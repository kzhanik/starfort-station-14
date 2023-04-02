using NUnit.Framework;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.UnitTesting.Server;

namespace Robust.UnitTesting.Shared.GameObjects
{
    [TestFixture, Parallelizable]
    sealed class EntityManagerTests
    {
        private static readonly MapId TestMapId = new(1);

        const string PROTOTYPE = @"
- type: entity
  name: dummy
  id: dummy
  components:
  - type: Transform
";

        private static ISimulation SimulationFactory()
        {
            var sim = RobustServerSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(PROTOTYPE))
                .InitializeInstance();

            var mapManager = sim.Resolve<IMapManager>();

            // Adds the map with id 1, and spawns entity 1 as the map entity.
            mapManager.CreateMap(TestMapId);

            return sim;
        }

        /// <summary>
        /// The entity prototype can define field on the TransformComponent, just like any other component.
        /// </summary>
        [Test]
        public void SpawnEntity_PrototypeTransform_Works()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var newEnt = entMan.SpawnEntity("dummy", new MapCoordinates(0, 0, TestMapId));
            Assert.That(newEnt, Is.Not.EqualTo(EntityUid.Invalid));
        }

        [Test]
        public void ComponentCount_Works()
        {
            var sim = RobustServerSimulation.NewSimulation().InitializeInstance();

            var entManager = sim.Resolve<IEntityManager>();
            var mapManager = sim.Resolve<IMapManager>();

            Assert.That(entManager.Count<TransformComponent>(), Is.EqualTo(0));

            var mapId = mapManager.CreateMap();
            Assert.That(entManager.Count<TransformComponent>(), Is.EqualTo(1));
            mapManager.DeleteMap(mapId);
            Assert.That(entManager.Count<TransformComponent>(), Is.EqualTo(0));
        }
    }
}
