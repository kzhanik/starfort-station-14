﻿using Robust.Client.Graphics;
using Robust.Shared.Maths;
using Robust.Shared.ViewVariables;

namespace Robust.Client.UserInterface.Controls
{
    public sealed class WindowRoot : UIRoot
    {
        internal WindowRoot(IClydeWindow window)
        {
            Window = window;
        }
        public override float UIScale => UIScaleSet;
        internal float UIScaleSet { get; set; }
        public override IClydeWindow Window { get; }
    }
}
