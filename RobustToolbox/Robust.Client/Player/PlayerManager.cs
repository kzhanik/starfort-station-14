using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Network.Messages;
using Robust.Shared.Players;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Robust.Client.Player
{
    /// <summary>
    ///     Here's the player controller. This will handle attaching GUIs and input to controllable things.
    ///     Why not just attach the inputs directly? It's messy! This makes the whole thing nicely encapsulated.
    ///     This class also communicates with the server to let the server control what entity it is attached to.
    /// </summary>
    public sealed class PlayerManager : IPlayerManager
    {
        [Dependency] private readonly IClientNetManager _network = default!;
        [Dependency] private readonly IBaseClient _client = default!;

        /// <summary>
        ///     Active sessions of connected clients to the server.
        /// </summary>
        private readonly Dictionary<NetUserId, ICommonSession> _sessions = new();

        /// <inheritdoc />
        public IEnumerable<ICommonSession> NetworkedSessions
        {
            get
            {
                if (LocalPlayer is not null)
                    return new[] {LocalPlayer.Session};

                return Enumerable.Empty<ICommonSession>();
            }
        }

        /// <inheritdoc />
        IEnumerable<ICommonSession> ISharedPlayerManager.Sessions => _sessions.Values;

        /// <inheritdoc />
        public int PlayerCount => _sessions.Values.Count;

        /// <inheritdoc />
        public int MaxPlayers => _client.GameInfo?.ServerMaxPlayers ?? 0;

        /// <inheritdoc />
        [ViewVariables]
        public LocalPlayer? LocalPlayer
        {
            get => _localPlayer;
            private set
            {
                if (_localPlayer == value) return;
                var oldValue = _localPlayer;
                _localPlayer = value;
                LocalPlayerChanged?.Invoke(new LocalPlayerChangedEventArgs(oldValue, _localPlayer));
            }
        }
        private LocalPlayer? _localPlayer;
        public event Action<LocalPlayerChangedEventArgs>? LocalPlayerChanged;

        /// <inheritdoc />
        [ViewVariables]
        IEnumerable<ICommonSession> IPlayerManager.Sessions => _sessions.Values;

        /// <inheritdoc />
        public IReadOnlyDictionary<NetUserId, ICommonSession> SessionsDict => _sessions;

        /// <inheritdoc />
        public event EventHandler? PlayerListUpdated;

        /// <inheritdoc />
        public void Initialize()
        {
            _client.RunLevelChanged += OnRunLevelChanged;

            _network.RegisterNetMessage<MsgPlayerListReq>();
            _network.RegisterNetMessage<MsgPlayerList>(HandlePlayerList);
        }

        /// <inheritdoc />
        public void Startup()
        {
            DebugTools.Assert(LocalPlayer == null);
            LocalPlayer = new LocalPlayer();

            var msgList = new MsgPlayerListReq();
            // message is empty
            _network.ClientSendMessage(msgList);
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            LocalPlayer?.DetachEntity();
            LocalPlayer = null;
            _sessions.Clear();
        }

        /// <inheritdoc />
        public void ApplyPlayerStates(IReadOnlyCollection<PlayerState> list)
        {
            if (list.Count == 0)
            {
                // This happens when the server says "nothing changed!"
                return;
            }
            DebugTools.Assert(_network.IsConnected ||  _client.RunLevel == ClientRunLevel.SinglePlayerGame // replays use state application.
                , "Received player state without being connected?");
            DebugTools.Assert(LocalPlayer != null, "Call Startup()");
            DebugTools.Assert(LocalPlayer!.Session != null, "Received player state before Session finished setup.");

            var myState = list.FirstOrDefault(s => s.UserId == LocalPlayer.UserId);

            if (myState != null)
            {
                UpdateAttachedEntity(myState.ControlledEntity);
                UpdateSessionStatus(myState.Status);
            }

            UpdatePlayerList(list);
        }

        /// <summary>
        ///     Compares the server sessionStatus to the client one, and updates if needed.
        /// </summary>
        private void UpdateSessionStatus(SessionStatus myStateStatus)
        {
            if (LocalPlayer!.Session.Status != myStateStatus)
                LocalPlayer.SwitchState(myStateStatus);
        }

        /// <summary>
        ///     Compares the server attachedEntity to the client one, and updates if needed.
        /// </summary>
        /// <param name="entity">AttachedEntity in the server session.</param>
        private void UpdateAttachedEntity(EntityUid? entity)
        {
            if (LocalPlayer!.ControlledEntity == entity)
            {
                return;
            }
            if (entity == null)
            {
                LocalPlayer.DetachEntity();
                return;
            }

            LocalPlayer.AttachEntity(entity.Value);
        }

        /// <summary>
        ///     Handles the incoming PlayerList message from the server.
        /// </summary>
        private void HandlePlayerList(MsgPlayerList msg)
        {
            UpdatePlayerList(msg.Plyrs);
        }

        /// <summary>
        ///     Compares the server player list to the client one, and updates if needed.
        /// </summary>
        private void UpdatePlayerList(IEnumerable<PlayerState> remotePlayers)
        {
            var dirty = false;

            var hitSet = new List<NetUserId>();

            foreach (var state in remotePlayers)
            {
                hitSet.Add(state.UserId);

                if (_sessions.TryGetValue(state.UserId, out var local))
                {
                    // Exists, update data.
                    if (local.Name == state.Name && local.Status == state.Status && local.Ping == state.Ping)
                        continue;

                    dirty = true;
                    local.Name = state.Name;
                    local.Status = state.Status;
                    local.Ping = state.Ping;
                }
                else
                {
                    // New, give him a slot.
                    dirty = true;

                    var newSession = new PlayerSession(state.UserId)
                    {
                        Name = state.Name,
                        Status = state.Status,
                        Ping = state.Ping
                    };
                    _sessions.Add(state.UserId, newSession);
                    if (state.UserId == LocalPlayer!.UserId)
                    {
                        LocalPlayer.InternalSession = newSession;
                        newSession.ConnectedClient = _network.ServerChannel!;
                        // We just connected to the server, hurray!
                        LocalPlayer.SwitchState(SessionStatus.Connecting, newSession.Status);
                    }
                }
            }

            foreach (var existing in _sessions.Keys.ToArray())
            {
                // clear slot, player left
                if (!hitSet.Contains(existing))
                {
                    DebugTools.Assert(LocalPlayer!.UserId != existing || _client.RunLevel == ClientRunLevel.SinglePlayerGame, // replays apply player states.
                        "I'm still connected to the server, but i left?");
                    _sessions.Remove(existing);
                    dirty = true;
                }
            }

            if (dirty)
            {
                PlayerListUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnRunLevelChanged(object? sender, RunLevelChangedEventArgs e)
        {
            if (e.NewLevel != ClientRunLevel.SinglePlayerGame)
                return;

            DebugTools.AssertNotNull(LocalPlayer);

            // We do some further setup steps for singleplayer here...

            // The local player's GUID in singleplayer will always be the default.
            var guid = default(NetUserId);

            var session = new PlayerSession(guid)
            {
                Name = LocalPlayer!.Name,
                Ping = 0,
            };

            LocalPlayer.UserId = guid;
            LocalPlayer.InternalSession = session;

            // Add the local session to the list.
            _sessions.Add(guid, session);

            LocalPlayer.SwitchState(SessionStatus.InGame);

            PlayerListUpdated?.Invoke(this, EventArgs.Empty);
        }

        public bool TryGetSessionByEntity(EntityUid uid, [NotNullWhen(true)] out ICommonSession? session)
        {
            if (LocalPlayer?.ControlledEntity == uid)
            {
                session = LocalPlayer.Session;
                return true;
            }

            session = null;
            return false;
        }
    }
}
