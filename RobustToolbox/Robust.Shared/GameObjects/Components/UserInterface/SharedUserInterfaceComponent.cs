using System;
using System.Collections.Generic;
using Robust.Shared.GameStates;
using Robust.Shared.Players;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Robust.Shared.GameObjects
{
    [NetworkedComponent]
    public abstract class SharedUserInterfaceComponent : Component
    {
        [DataField("interfaces")]
        internal List<PrototypeData> _interfaceData = new();

        [DataDefinition]
        public sealed class PrototypeData
        {
            [DataField("key", readOnly: true, required: true)]
            public Enum UiKey { get; set; } = default!;

            [DataField("type", readOnly: true, required: true)]
            public string ClientType { get; set; } = default!;

            /// <summary>
            ///     Maximum range before a BUI auto-closes. A non-positive number means there is no limit.
            /// </summary>
            [DataField("range")]
            public float InteractionRange = 2f;

            // TODO BUI move to content?
            // I've tried to keep the name general, but really this is a bool for: can ghosts/stunned/dead people press buttons on this UI?
            /// <summary>
            ///     Determines whether the server should verify that a client is capable of performing generic UI interactions when receiving UI messages.
            /// </summary>
            /// <remarks>
            ///     Avoids requiring each system to individually validate client inputs. However, perhaps some BUIs are supposed to be bypass accessibility checks
            /// </remarks>
            [DataField("requireInputValidation")]
            public bool RequireInputValidation = true;
        }
    }

    /// <summary>
    ///     Raised whenever the server receives a BUI message from a client relating to a UI that requires input
    ///     validation.
    /// </summary>
    public sealed class BoundUserInterfaceMessageAttempt : CancellableEntityEventArgs
    {
        public readonly ICommonSession Sender;
        public readonly EntityUid Target;
        public readonly Enum UiKey;

        public BoundUserInterfaceMessageAttempt(ICommonSession sender, EntityUid target, Enum uiKey)
        {
            Sender = sender;
            Target = target;
            UiKey = uiKey;
        }
    }

    [NetSerializable, Serializable]
    public abstract class BoundUserInterfaceState
    {
    }


    [NetSerializable, Serializable]
    public abstract class BoundUserInterfaceMessage : EntityEventArgs
    {
        /// <summary>
        ///     The UI of this message.
        ///     Only set when the message is raised as a directed event.
        /// </summary>
        public Enum UiKey { get; set; } = default!;

        /// <summary>
        ///     The Entity receiving the message.
        ///     Only set when the message is raised as a directed event.
        /// </summary>
        public EntityUid Entity { get; set; } = EntityUid.Invalid;

        /// <summary>
        ///     The session sending or receiving this message.
        ///     Only set when the message is raised as a directed event.
        /// </summary>
        public ICommonSession Session { get; set; } = default!;
    }

    [NetSerializable, Serializable]
    internal sealed class UpdateBoundStateMessage : BoundUserInterfaceMessage
    {
        public readonly BoundUserInterfaceState State;

        public UpdateBoundStateMessage(BoundUserInterfaceState state)
        {
            State = state;
        }
    }

    [NetSerializable, Serializable]
    internal sealed class OpenBoundInterfaceMessage : BoundUserInterfaceMessage
    {
    }

    [NetSerializable, Serializable]
    internal sealed class CloseBoundInterfaceMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    internal sealed class BoundUIWrapMessage : EntityEventArgs
    {
        public readonly EntityUid Entity;
        public readonly BoundUserInterfaceMessage Message;
        public readonly Enum UiKey;

        public BoundUIWrapMessage(EntityUid entity, BoundUserInterfaceMessage message, Enum uiKey)
        {
            Message = message;
            UiKey = uiKey;
            Entity = entity;
        }

        public override string ToString()
        {
            return $"{nameof(BoundUIWrapMessage)}: {Message}";
        }
    }

    public sealed class BoundUIOpenedEvent : BoundUserInterfaceMessage
    {
        public BoundUIOpenedEvent(Enum uiKey, EntityUid uid, ICommonSession session)
        {
            UiKey = uiKey;
            Entity = uid;
            Session = session;
        }
    }

    public sealed class BoundUIClosedEvent : BoundUserInterfaceMessage
    {
        public BoundUIClosedEvent(Enum uiKey, EntityUid uid, ICommonSession session)
        {
            UiKey = uiKey;
            Entity = uid;
            Session = session;
        }
    }
}
