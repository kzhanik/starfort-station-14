﻿using Robust.Shared.Serialization;
using System;
using System.Diagnostics.Contracts;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.GameObjects;

namespace Robust.Shared.Audio
{
    public enum Attenuation : int
    {
        // https://hackage.haskell.org/package/OpenAL-1.7.0.5/docs/Sound-OpenAL-AL-Attenuation.html

        /// <summary>
        /// Default to the overall attenuation. If set project-wide will use InverseDistanceClamped. This is what you typically want for an audio source.
        /// </summary>
        Default = 0,
        NoAttenuation = 1 << 0,
        InverseDistance = 1 << 1,
        InverseDistanceClamped = 1 << 2,
        LinearDistance = 1 << 3,
        LinearDistanceClamped = 1 << 4,
        ExponentDistance = 1 << 5,
        ExponentDistanceClamped = 1 << 6,
    }

    /// <summary>
    ///     Contains common audio parameters for audio playback on the client.
    /// </summary>
    [Serializable, NetSerializable]
    [DataDefinition]
    public struct AudioParams
    {
        /// <summary>
        ///     The DistanceModel to use for this specific source.
        /// </summary>
        [DataField("attenuation")]
        public Attenuation Attenuation { get; set; } = Default.Attenuation;

        /// <summary>
        ///     Base volume to play the audio at, in dB.
        /// </summary>
        [DataField("volume")]
        public float Volume { get; set; } = Default.Volume;

        /// <summary>
        ///     Scale for the audio pitch.
        /// </summary>
        [DataField("pitchscale")]
        public float PitchScale { get; set; } = Default.PitchScale;

        /// <summary>
        ///     Audio bus to play on.
        /// </summary>
        [DataField("busname")]
        public string BusName { get; set; } = Default.BusName;

        /// <summary>
        ///     Only applies to positional audio.
        ///     The maximum distance from which the audio is hearable.
        /// </summary>
        [DataField("maxdistance")]
        public float MaxDistance { get; set; } = Default.MaxDistance;

        /// <summary>
        ///     Used for distance attenuation calculations. Set to 0f to make a sound exempt from distance attenuation.
        /// </summary>
        [DataField("rolloffFactor")]
        public float RolloffFactor { get; set; } = Default.RolloffFactor;

        /// <summary>
        ///     Equivalent of the minimum distance to use for an audio source.
        /// </summary>
        [DataField("referenceDistance")]
        public float ReferenceDistance { get; set; } = Default.ReferenceDistance;

        [DataField("loop")] public bool Loop { get; set; } = Default.Loop;

        [DataField("playoffset")] public float PlayOffsetSeconds { get; set; } = Default.PlayOffsetSeconds;

        /// <summary>
        ///     If not null, this will randomly modify the pitch scale by adding a number drawn from a normal distribution with this deviation.
        /// </summary>
        [DataField("variation")]
        public float? Variation { get; set; } = null;

        // For the max distance value: it's 2000 in Godot, but I assume that's PIXELS due to the 2D positioning,
        // so that's divided by 32 (EyeManager.PIXELSPERMETER).
        /// <summary>
        ///     The "default" audio configuration.
        /// </summary>
        public static readonly AudioParams Default = new(0, 1, "Master", SharedAudioSystem.DefaultSoundRange, 1, 1, false, 0f);

        // explicit parameterless constructor required so that default values get set properly.
        public AudioParams() { }

        public AudioParams(
            float volume,
            float pitchScale,
            string busName,
            float maxDistance,
            float refDistance,
            bool loop,
            float playOffsetSeconds,
            float? variation = null)
            : this(volume, pitchScale, busName, maxDistance, 1, refDistance, loop, playOffsetSeconds, variation)
        {
        }

        public AudioParams(float volume, float pitchScale, string busName, float maxDistance,float rolloffFactor, float refDistance, bool loop, float playOffsetSeconds, float? variation = null) : this()
        {
            Volume = volume;
            PitchScale = pitchScale;
            BusName = busName;
            MaxDistance = maxDistance;
            RolloffFactor = rolloffFactor;
            ReferenceDistance = refDistance;
            Loop = loop;
            PlayOffsetSeconds = playOffsetSeconds;
            Variation = variation;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new volume set, for easy chaining.
        /// </summary>
        /// <param name="volume">The new volume.</param>
        [Pure]
        public readonly AudioParams WithVolume(float volume)
        {
            var me = this;
            me.Volume = volume;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a modified volume set, for easy chaining.
        /// </summary>
        /// <param name="volume">The volume to add.</param>
        [Pure]
        public readonly AudioParams AddVolume(float volume)
        {
            var me = this;
            me.Volume += volume;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new variation set, for easy chaining.
        /// </summary>
        [Pure]
        public readonly AudioParams WithVariation(float? variation)
        {
            var me = this;
            me.Variation = variation;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new pitch scale set, for easy chaining.
        /// </summary>
        /// <param name="pitch">The new pitch scale.</param>
        [Pure]
        public readonly AudioParams WithPitchScale(float pitch)
        {
            var me = this;
            me.PitchScale = pitch;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new bus name set, for easy chaining.
        /// </summary>
        /// <param name="bus">The new bus name.</param>
        [Pure]
        public readonly AudioParams WithBusName(string bus)
        {
            var me = this;
            me.BusName = bus;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new max distance set, for easy chaining.
        /// </summary>
        /// <param name="dist">The new max distance.</param>
        [Pure]
        public readonly AudioParams WithMaxDistance(float dist)
        {
            var me = this;
            me.MaxDistance = dist;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new rolloff factor set, for easy chaining.
        /// </summary>
        /// <param name="rolloffFactor">The new rolloff factor.</param>
        [Pure]
        public readonly AudioParams WithRolloffFactor(float rolloffFactor)
        {
            var me = this;
            me.RolloffFactor = rolloffFactor;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a new reference distance set, for easy chaining.
        /// </summary>
        /// <param name="refDistance">The new reference distance.</param>
        [Pure]
        public readonly AudioParams WithReferenceDistance(float refDistance)
        {
            var me = this;
            me.ReferenceDistance = refDistance;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with a loop set, for easy chaining.
        /// </summary>
        /// <param name="loop">The new loop.</param>
        [Pure]
        public readonly AudioParams WithLoop(bool loop)
        {
            var me = this;
            me.Loop = loop;
            return me;
        }

        /// <summary>
        ///     Returns a copy of this instance with attenuation set, for easy chaining.
        /// </summary>
        /// <param name="attenuation">The new attenuation.</param>
        [Pure]
        public readonly AudioParams WithAttenuation(Attenuation attenuation)
        {
            var me = this;
            me.Attenuation = attenuation;
            return me;
        }

        [Pure]
        public readonly AudioParams WithPlayOffset(float offset)
        {
            var me = this;
            me.PlayOffsetSeconds = offset;
            return me;
        }
    }
}
