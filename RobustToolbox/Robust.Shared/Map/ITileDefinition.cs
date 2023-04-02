﻿using System.Collections.Generic;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Robust.Shared.Map
{
    /// <summary>
    ///     The definition (template) for a grid tile.
    /// </summary>
    public interface ITileDefinition
    {
        /// <summary>
        ///     The numeric tile ID used to refer to this tile inside the map datastructure.
        /// </summary>
        ushort TileId { get; }

        /// <summary>
        ///     The name of the definition. This is user facing.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Internal name of the definition.
        /// </summary>
        string ID { get; }

        /// <summary>
        ///     The path of the sprite to draw.
        /// </summary>
        ResourcePath? Sprite { get; }

        /// <summary>
        /// Possible sprites to use if we're neighboring another tile.
        /// </summary>
        Dictionary<Direction, ResourcePath> EdgeSprites { get; }

        /// <summary>
        ///     Physics objects that are interacting on this tile are slowed down by this float.
        /// </summary>
        float Friction { get; }

        /// <summary>
        ///     Number of variants this tile has. ALSO DETERMINES THE EXPECTED INPUT TEXTURE SIZE.
        /// </summary>
        byte Variants { get; }

        /// <summary>
        ///     Assign a new value to <see cref="TileId"/>, used when registering the tile definition.
        /// </summary>
        /// <param name="id">The new tile ID for this tile definition.</param>
        void AssignTileId(ushort id);
    }
}
