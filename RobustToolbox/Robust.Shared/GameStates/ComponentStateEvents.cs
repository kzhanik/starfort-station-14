using Robust.Shared.GameObjects;
using Robust.Shared.Players;
using Robust.Shared.Timing;

namespace Robust.Shared.GameStates
{
    [ByRefEvent, ComponentEvent]
    public readonly struct ComponentHandleState
    {
        public ComponentState? Current { get; }
        public ComponentState? Next { get; }

        public ComponentHandleState(ComponentState? current, ComponentState? next)
        {
            Current = current;
            Next = next;
        }
    }

    /// <summary>
    ///     Component event for getting the component state for a specific player.
    /// </summary>
    [ByRefEvent, ComponentEvent]
    public struct ComponentGetState
    {
        public GameTick FromTick { get; }

        /// <summary>
        ///     Output parameter. Set this to the component's state for the player.
        /// </summary>
        public ComponentState? State { get; set; }

        /// <summary>
        ///     If true, this state is intended for replays or some other server spectator entity, not for specific
        ///     clients.
        /// </summary>
        public bool ReplayState => Player == null;

        /// <summary>
        ///     The player the state is being sent to. Null implies the state is for a replay or some spectator entity.
        /// </summary>
        public readonly ICommonSession? Player;

        public ComponentGetState(ICommonSession? player, GameTick fromTick)
        {
            Player = player;
            FromTick = fromTick;
            State = null;
        }
    }

    [ByRefEvent, ComponentEvent]
    public struct ComponentGetStateAttemptEvent
    {
        /// <summary>
        ///     Input parameter. The player the state is being sent to. This may be null if the state is for replay recordings.
        /// </summary>
        public readonly ICommonSession? Player;

        public bool Cancelled = false;

        public ComponentGetStateAttemptEvent(ICommonSession? player)
        {
            Player = player;
        }
    }
}
