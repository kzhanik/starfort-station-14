using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Utility;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Robust.UnitTesting.Server.GameObjects.Components
{
    [TestFixture, Parallelizable]
    public sealed class ContainerTest
    {
        private static ISimulation SimulationFactory()
        {
            var sim = RobustServerSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            // Adds the map with id 1, and spawns entity 1 as the map entity.
            sim.AddMap(1);

            return sim;
        }

        const string Prototypes = @"
- type: entity
  name: dummy
  id: dummy
";

        [Test]
        public void TestCreation()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));

            var container = containerSys.MakeContainer<Container>(entity, "dummy");

            Assert.That(container.ID, Is.EqualTo("dummy"));
            Assert.That(container.Owner, Is.EqualTo(entity));

            var manager = IoCManager.Resolve<IEntityManager>().GetComponent<IContainerManager>(entity);

            Assert.That(container.Manager, Is.EqualTo(manager));
            Assert.That(() => containerSys.MakeContainer<Container>(entity, "dummy"), Throws.ArgumentException);

            Assert.That(manager.HasContainer("dummy2"), Is.False);
            var container2 = containerSys.MakeContainer<Container>(entity, "dummy2");

            Assert.That(container2.Manager, Is.EqualTo(manager));
            Assert.That(container2.Owner, Is.EqualTo(entity));
            Assert.That(container2.ID, Is.EqualTo("dummy2"));

            Assert.That(manager.HasContainer("dummy"), Is.True);
            Assert.That(manager.HasContainer("dummy2"), Is.True);
            Assert.That(manager.HasContainer("dummy3"), Is.False);

            Assert.That(manager.GetContainer("dummy"), Is.EqualTo(container));
            Assert.That(manager.GetContainer("dummy2"), Is.EqualTo(container2));
            Assert.That(() => manager.GetContainer("dummy3"), Throws.TypeOf<KeyNotFoundException>());

            IoCManager.Resolve<IEntityManager>().DeleteEntity(entity);

            Assert.That(manager.Deleted, Is.True);
            Assert.That(container.Deleted, Is.True);
            Assert.That(container2.Deleted, Is.True);
        }

        [Test]
        public void TestInsertion()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var owner = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var inserted = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var transform = IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(inserted);

            var container = containerSys.MakeContainer<Container>(owner, "dummy");
            Assert.That(container.Insert(inserted), Is.True);
            Assert.That(transform.ParentUid, Is.EqualTo(owner));

            var container2 = containerSys.MakeContainer<Container>(inserted, "dummy");
            Assert.That(container2.Insert(owner), Is.False);

            var success = container.Remove(inserted);
            Assert.That(success, Is.True);

            success = container.Remove(inserted);
            Assert.That(success, Is.False);

            container.Insert(inserted);
            IoCManager.Resolve<IEntityManager>().DeleteEntity(owner);
            // Make sure inserted was detached.
            Assert.That(transform.Deleted, Is.True);
        }

        [Test]
        public void TestNestedRemoval()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var owner = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var inserted = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var transform = IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(inserted);
            var entity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));

            var container = containerSys.MakeContainer<Container>(owner, "dummy");
            Assert.That(container.Insert(inserted), Is.True);
            Assert.That(transform.ParentUid, Is.EqualTo(owner));

            var container2 = containerSys.MakeContainer<Container>(inserted, "dummy");
            Assert.That(container2.Insert(entity), Is.True);
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(entity).ParentUid, Is.EqualTo(inserted));

            Assert.That(container2.Remove(entity), Is.True);
            Assert.That(container.Contains(entity), Is.True);
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(entity).ParentUid, Is.EqualTo(owner));

            IoCManager.Resolve<IEntityManager>().DeleteEntity(owner);
            Assert.That(transform.Deleted, Is.True);
        }

        [Test]
        public void TestNestedRemovalWithDenial()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var coordinates = new EntityCoordinates(new EntityUid(1), (0, 0));
            var entityOne = sim.SpawnEntity("dummy", coordinates);
            var entityTwo = sim.SpawnEntity("dummy", coordinates);
            var entityThree = sim.SpawnEntity("dummy", coordinates);
            var entityItem = sim.SpawnEntity("dummy", coordinates);

            var container = containerSys.MakeContainer<Container>(entityOne, "dummy");
            var container2 = containerSys.MakeContainer<ContainerOnlyContainer>(entityTwo, "dummy");
            var container3 = containerSys.MakeContainer<Container>(entityThree, "dummy");

            var entMan = IoCManager.Resolve<IEntityManager>();

            Assert.That(container.Insert(entityTwo), Is.True);
            Assert.That(entMan.GetComponent<TransformComponent>(entityTwo).ParentUid, Is.EqualTo(entityOne));

            Assert.That(container2.Insert(entityThree), Is.True);
            Assert.That(entMan.GetComponent<TransformComponent>(entityThree).ParentUid, Is.EqualTo(entityTwo));

            Assert.That(container3.Insert(entityItem), Is.True);
            Assert.That(entMan.GetComponent<TransformComponent>(entityItem).ParentUid, Is.EqualTo(entityThree));

            Assert.That(container3.Remove(entityItem), Is.True);
            Assert.That(container.Contains(entityItem), Is.True);
            Assert.That(entMan.GetComponent<TransformComponent>(entityItem).ParentUid, Is.EqualTo(entityOne));

            entMan.DeleteEntity(entityOne);
            Assert.That(entMan.Deleted(entityOne), Is.True);
        }

        [Test]
        public void BaseContainer_SelfInsert_False()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container = containerSys.MakeContainer<Container>(entity, "dummy");

            Assert.That(container.Insert(entity), Is.False);
            Assert.That(container.CanInsert(entity), Is.False);
        }

        [Test]
        public void BaseContainer_InsertMap_False()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var mapEnt = new EntityUid(1);
            var entity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container = containerSys.MakeContainer<Container>(entity, "dummy");

            Assert.That(container.Insert(mapEnt), Is.False);
            Assert.That(container.CanInsert(mapEnt), Is.False);
        }

        [Test]
        public void BaseContainer_InsertGrid_False()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var grid = sim.Resolve<IMapManager>().CreateGrid(new MapId(1)).Owner;
            var entity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container = containerSys.MakeContainer<Container>(entity, "dummy");

            Assert.That(container.Insert(grid), Is.False);
            Assert.That(container.CanInsert(grid), Is.False);
        }

        [Test]
        public void BaseContainer_Insert_True()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var containerEntity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container = containerSys.MakeContainer<Container>(containerEntity, "dummy");
            var insertEntity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));

            var result = container.Insert(insertEntity);

            Assert.That(result, Is.True);
            Assert.That(container.ContainedEntities.Count, Is.EqualTo(1));

            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(containerEntity).ChildCount, Is.EqualTo(1));
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(containerEntity).ChildEntities.First(), Is.EqualTo(insertEntity));

            result = insertEntity.TryGetContainerMan(out var resultContainerMan);
            Assert.That(result, Is.True);
            Assert.That(resultContainerMan, Is.EqualTo(container.Manager));
        }

        [Test]
        public void BaseContainer_RemoveNotAdded_False()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var containerEntity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container = containerSys.MakeContainer<Container>(containerEntity, "dummy");
            var insertEntity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));

            var result = container.Remove(insertEntity);

            Assert.That(result, Is.False);
        }

        [Test]
        public void BaseContainer_Transfer_True()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity1 = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container1 = containerSys.MakeContainer<Container>(entity1, "dummy");
            var entity2 = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container2 = containerSys.MakeContainer<Container>(entity2, "dummy");
            var transferEntity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            container1.Insert(transferEntity);

            var result = container2.Insert(transferEntity);

            Assert.That(result, Is.True);
            Assert.That(container1.ContainedEntities.Count, Is.EqualTo(0));
            Assert.That(container2.ContainedEntities.Count, Is.EqualTo(1));
        }

        [Test]
        public void Container_Serialize()
        {
            var sim = SimulationFactory();
            var containerSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity = sim.SpawnEntity("dummy", new EntityCoordinates(new EntityUid(1), (0, 0)));
            var container = containerSys.MakeContainer<Container>(entity, "dummy");
            var childEnt = sim.SpawnEntity(null, new EntityCoordinates(new EntityUid(1), (0, 0)));

            container.OccludesLight = true;
            container.ShowContents = true;
            container.Insert(childEnt);

            var containerMan = IoCManager.Resolve<IEntityManager>().GetComponent<IContainerManager>(entity);
            var state = (ContainerManagerComponent.ContainerManagerComponentState)containerMan.GetComponentState();

            Assert.That(state.Containers.Count, Is.EqualTo(1));
            var cont = state.Containers.Values.First();
            Assert.That(cont.Id, Is.EqualTo("dummy"));
            Assert.That(cont.OccludesLight, Is.True);
            Assert.That(cont.ShowContents, Is.True);
            Assert.That(cont.ContainedEntities.Length, Is.EqualTo(1));
            Assert.That(cont.ContainedEntities[0], Is.EqualTo(childEnt));
        }

        private sealed class ContainerOnlyContainer : BaseContainer
        {
            /// <summary>
            /// The generic container class uses a list of entities
            /// </summary>
            private readonly List<EntityUid> _containerList = new();
            private readonly List<EntityUid> _expectedEntities = new();

            public override string ContainerType => nameof(ContainerOnlyContainer);

            /// <inheritdoc />
            public override IReadOnlyList<EntityUid> ContainedEntities => _containerList;

            public override List<EntityUid> ExpectedEntities => _expectedEntities;

            /// <inheritdoc />
            protected override void InternalInsert(EntityUid toInsert, IEntityManager entMan)
            {
                _containerList.Add(toInsert);
            }

            /// <inheritdoc />
            protected override void InternalRemove(EntityUid toRemove, IEntityManager entMan)
            {
                _containerList.Remove(toRemove);
            }

            /// <inheritdoc />
            public override bool Contains(EntityUid contained)
            {
                if (!_containerList.Contains(contained))
                    return false;

                var flags = IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(contained).Flags;
                DebugTools.Assert((flags & MetaDataFlags.InContainer) != 0);
                return true;
            }

            /// <inheritdoc />
            protected override void InternalShutdown(IEntityManager entMan, bool isClient)
            {
                foreach (var entity in _containerList.ToArray())
                {
                    if (!isClient)
                        entMan.DeleteEntity(entity);
                    else if (entMan.EntityExists(entity))
                        Remove(entity, entMan, reparent: false, force: true);
                }
            }

            public override bool CanInsert(EntityUid toinsert, IEntityManager? entMan = null)
            {
                IoCManager.Resolve(ref entMan);
                return entMan.TryGetComponent(toinsert, out IContainerManager? _);
            }
        }
    }
}
