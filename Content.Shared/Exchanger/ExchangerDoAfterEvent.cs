﻿using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Exchanger;

[Serializable, NetSerializable]
public sealed class ExchangerDoAfterEvent : SimpleDoAfterEvent
{
}