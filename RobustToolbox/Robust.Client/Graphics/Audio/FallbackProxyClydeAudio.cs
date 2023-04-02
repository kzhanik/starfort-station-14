﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Robust.Client.Audio;
using Robust.Client.Input;
using Robust.Shared.Timing;
using Robust.Shared.IoC;

namespace Robust.Client.Graphics.Audio
{
    /// <summary>
    ///     For "start ss14 with no audio devices" Smugleaf
    /// </summary>
    [UsedImplicitly]
    internal sealed class FallbackProxyClydeAudio : ProxyClydeAudio
    {
        [Dependency] private readonly IDependencyCollection _deps = default!;

        public override bool InitializePostWindowing()
        {
            // Deliberate lack of base call here (see base implementation for comments as to why there even is a base)

            ActualImplementation = new ClydeAudio();
            _deps.InjectDependencies(ActualImplementation, true);
            if (ActualImplementation.InitializePostWindowing())
                return true;

            // If we get here, that failed, so use the fallback
            ActualImplementation = new ClydeAudioHeadless();
            _deps.InjectDependencies(ActualImplementation, true);
            return ActualImplementation.InitializePostWindowing();
        }
    }
}
