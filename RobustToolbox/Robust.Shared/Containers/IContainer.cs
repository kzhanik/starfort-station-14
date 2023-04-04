using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Robust.Shared.Containers
{
    /// <summary>
    /// A container is a way to "contain" entities inside other entities, in a logical way.
    /// This is alike BYOND's <c>contents</c> system, except more advanced.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///     Containers are logical separations of entities contained inside another entity.
    ///     for example, a crate with two separated compartments would have two separate containers.
    ///     If an entity inside compartment A drops something,
    ///     the dropped entity would be placed in compartment A too,
    ///     and compartment B would be completely untouched.
    ///     </p>
    ///     <p>
    ///     Containers are managed by an entity's <see cref="IContainerManager" />,
    ///     and have an ID to be referenced by.
    ///     </p>
    /// </remarks>
    /// <seealso cref="IContainerManager" />
    [PublicAPI]
    [ImplicitDataDefinitionForInheritors]
    public interface IContainer
    {
        /// <summary>
        /// Readonly collection of all the entities contained within this specific container
        /// </summary>
        IReadOnlyList<EntityUid> ContainedEntities { get; }

        List<EntityUid> ExpectedEntities { get; }

        /// <summary>
        /// The type of this container.
        /// </summary>
        string ContainerType { get; }

        /// <summary>
        /// True if the container has been shut down via <see cref="Shutdown" />
        /// </summary>
        bool Deleted { get; }

        /// <summary>
        /// The ID of this container.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Prevents light from escaping the container, from ex. a flashlight.
        /// </summary>
        bool OccludesLight { get; set; }

        /// <summary>
        /// The entity owning this container.
        /// </summary>
        EntityUid Owner { get; }

        /// <summary>
        /// Should the contents of this container be shown? False for closed containers like lockers, true for
        /// things like glass display cases.
        /// </summary>
        bool ShowContents { get; set; }

        /// <summary>
        /// Checks if the entity can be inserted into this container.
        /// </summary>
        /// <param name="toinsert">The entity to attempt to insert.</param>
        /// <param name="entMan"></param>
        /// <returns>True if the entity can be inserted, false otherwise.</returns>
        bool CanInsert(EntityUid toinsert, IEntityManager? entMan = null);

        /// <summary>
        /// Attempts to insert the entity into this container.
        /// </summary>
        /// <remarks>
        /// If the insertion is successful, the inserted entity will end up parented to the
        /// container entity, and the inserted entity's local position will be set to the zero vector.
        /// </remarks>
        /// <param name="toinsert">The entity to insert.</param>
        /// <param name="entMan"></param>
        /// <returns>False if the entity could not be inserted.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this container is a child of the entity,
        /// which would cause infinite loops.
        /// </exception>
        bool Insert(EntityUid toinsert,
            IEntityManager? entMan = null,
            TransformComponent? transform = null,
            TransformComponent? ownerTransform = null,
            MetaDataComponent? meta = null,
            PhysicsComponent? physics = null,
            bool force = false);

        /// <summary>
        /// Checks if the entity can be removed from this container.
        /// </summary>
        /// <param name="toremove">The entity to check.</param>
        /// <param name="entMan"></param>
        /// <returns>True if the entity can be removed, false otherwise.</returns>
        bool CanRemove(EntityUid toremove, IEntityManager? entMan = null);

        /// <summary>
        /// Attempts to remove the entity from this container.
        /// </summary>
        /// <param name="reparent">If false, this operation will not rigger a move or parent change event. Ignored if
        /// destination is not null</param>
        /// <param name="force">If true, this will not perform can-remove checks.</param>
        /// <param name="destination">Where to place the entity after removing. Avoids unnecessary broadphase updates.
        /// If not specified, and reparent option is true, then the entity will either be inserted into a parent
        /// container, the grid, or the map.</param>
        /// <param name="localRotation">Optional final local rotation after removal. Avoids redundant move events.</param>
        bool Remove(
            EntityUid toremove,
            IEntityManager? entMan = null,
            TransformComponent? xform = null,
            MetaDataComponent? meta = null,
            bool reparent = true,
            bool force = false,
            EntityCoordinates? destination = null,
            Angle? localRotation = null);

        [Obsolete("use force option in Remove()")]
        void ForceRemove(EntityUid toRemove, IEntityManager? entMan = null, MetaDataComponent? meta = null);

        /// <summary>
        /// Checks if the entity is contained in this container.
        /// This is not recursive, so containers of children are not checked.
        /// </summary>
        /// <param name="contained">The entity to check.</param>
        /// <returns>True if the entity is immediately contained in this container, false otherwise.</returns>
        bool Contains(EntityUid contained);

        /// <summary>
        /// Clears the container and marks it as deleted.
        /// </summary>
        void Shutdown(IEntityManager? entMan = null, INetManager? netMan = null);
    }
}
