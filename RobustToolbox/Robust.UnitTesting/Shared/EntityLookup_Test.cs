using System.Linq;
using NUnit.Framework;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.UnitTesting.Server;

namespace Robust.UnitTesting.Shared
{
    [TestFixture, TestOf(typeof(EntityLookupSystem))]
    public sealed class EntityLookupTest
    {
        [Test]
        public void AnyIntersecting()
        {
            var sim = RobustServerSimulation.NewSimulation();
            var server = sim.InitializeInstance();

            var lookup = server.Resolve<IEntitySystemManager>().GetEntitySystem<EntityLookupSystem>();
            var entManager = server.Resolve<IEntityManager>();
            var mapManager = server.Resolve<IMapManager>();

            var mapId = mapManager.CreateMap();

            var theMapSpotBeingUsed = new Box2(Vector2.Zero, Vector2.One);

            var dummy = entManager.SpawnEntity(null, new MapCoordinates(Vector2.Zero, mapId));
            Assert.That(lookup.AnyEntitiesIntersecting(mapId, theMapSpotBeingUsed));
            mapManager.DeleteMap(mapId);
        }

        /// <summary>
        /// Is the entity correctly removed / added to EntityLookup when anchored
        /// </summary>
        [Test]
        public void TestAnchoring()
        {
            var sim = RobustServerSimulation.NewSimulation();
            // sim.RegisterEntitySystems(m => m.LoadExtraSystemType<EntityLookupSystem>());
            var server = sim.InitializeInstance();

            var lookup = server.Resolve<IEntitySystemManager>().GetEntitySystem<EntityLookupSystem>();
            var entManager = server.Resolve<IEntityManager>();
            var mapManager = server.Resolve<IMapManager>();

            var mapId = mapManager.CreateMap();
            var grid = mapManager.CreateGrid(mapId);

            var theMapSpotBeingUsed = new Box2(Vector2.Zero, Vector2.One);
            grid.SetTile(new Vector2i(), new Tile(1));

            Assert.That(lookup.GetEntitiesIntersecting(mapId, theMapSpotBeingUsed).ToList().Count, Is.EqualTo(0));

            // Setup and check it actually worked
            var dummy = entManager.SpawnEntity(null, new MapCoordinates(Vector2.Zero, mapId));
            Assert.That(lookup.GetEntitiesIntersecting(mapId, theMapSpotBeingUsed).ToList().Count, Is.EqualTo(1));

            var xform = entManager.GetComponent<TransformComponent>(dummy);

            // When anchoring it should still get returned.
            xform.Anchored = true;
            Assert.That(xform.Anchored);
            Assert.That(lookup.GetEntitiesIntersecting(mapId, theMapSpotBeingUsed).ToList().Count, Is.EqualTo(1));

            xform.Anchored = false;
            Assert.That(lookup.GetEntitiesIntersecting(mapId, theMapSpotBeingUsed).ToList().Count, Is.EqualTo(1));

            entManager.DeleteEntity(dummy);
            mapManager.DeleteGrid(grid.Owner);
            mapManager.DeleteMap(mapId);
        }
    }
}
