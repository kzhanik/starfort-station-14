﻿using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Robust.Client.GameObjects;

/// <summary>
///     Handles the visualization of data inside of an appearance component.
///     Implementations of this class are NOT bound to a specific entity, they are flyweighted across multiple.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract class AppearanceVisualizer
{
    /// <summary>
    ///     Initializes an entity to be managed by this appearance controller.
    ///     DO NOT assume this is your only entity. Visualizers are shared.
    /// </summary>
    [Obsolete("Subscribe to your component being initialised instead.")]
    public virtual void InitializeEntity(EntityUid entity)
    {
    }

    /// <summary>
    ///     Called whenever appearance data for an entity changes.
    ///     Update its visuals here.
    /// </summary>
    /// <param name="component">The appearance component of the entity that might need updating.</param>
    [Obsolete("Subscribe to AppearanceChangeEvent instead.")]
    public virtual void OnChangeData(AppearanceComponent component)
    {
    }
}
