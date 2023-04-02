using System.Threading.Tasks;
using NUnit.Framework;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Robust.UnitTesting.Shared.Physics;

/// <summary>
/// Tests moving and deleting a grid.
/// Mainly useful for grid dynamic tree.
/// </summary>
[TestFixture]
public sealed class GridDeletion_Test : RobustIntegrationTest
{
    [Test]
    public async Task GridDeletionTest()
    {
        var server = StartServer();

        await server.WaitIdleAsync();

        var entManager = server.ResolveDependency<IEntityManager>();
        var mapManager = server.ResolveDependency<IMapManager>();
        var physSystem = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<SharedPhysicsSystem>();


        PhysicsComponent physics = default!;
        MapGridComponent grid = default!;
        MapId mapId = default!;

        await server.WaitAssertion(() =>
        {
            mapId = mapManager.CreateMap();
            grid = mapManager.CreateGrid(mapId);

            physics = entManager.GetComponent<PhysicsComponent>(grid.Owner);
            physSystem.SetBodyType(grid.Owner, BodyType.Dynamic, body: physics);
            physSystem.SetLinearVelocity(grid.Owner, new Vector2(50f, 0f), body: physics);
            Assert.That(physics.LinearVelocity.Length, NUnit.Framework.Is.GreaterThan(0f));
        });

        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            Assert.That(physics.LinearVelocity.Length, NUnit.Framework.Is.GreaterThan(0f));
            entManager.DeleteEntity(grid.Owner);

            // So if gridtree is fucky then this SHOULD throw.
            foreach (var _ in mapManager.FindGridsIntersecting(mapId,
                         new Box2(new Vector2(float.MinValue, float.MinValue),
                             new Vector2(float.MaxValue, float.MaxValue))))
            {
            }
        });
    }
}
