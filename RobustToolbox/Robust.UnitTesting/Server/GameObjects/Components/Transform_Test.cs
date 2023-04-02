using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Server.Physics;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Robust.UnitTesting.Server.GameObjects.Components
{
    [TestFixture]
    [TestOf(typeof(TransformComponent))]
    sealed class Transform_Test : RobustUnitTest
    {
        public override UnitTestProject Project => UnitTestProject.Server;

        private IServerEntityManagerInternal EntityManager = default!;
        private IMapManager MapManager = default!;

        const string PROTOTYPES = @"
- type: entity
  name: dummy
  id: dummy
  components:
  - type: Transform

- type: entity
  name: dummy
  id: mapDummy
  components:
  - type: Transform
  - type: Map
    index: 123
  # Due to the map getting initialised last this seemed easiest to fix the test while removing the mocks.
  - type: Broadphase
";

        private MapId MapA;
        private MapGridComponent GridA = default!;
        private MapId MapB;
        private MapGridComponent GridB = default!;

        private static readonly EntityCoordinates InitialPos = new(new EntityUid(1), (0, 0));

        [OneTimeSetUp]
        public void Setup()
        {
            IoCManager.Resolve<IComponentFactory>().GenerateNetIds();

            EntityManager = IoCManager.Resolve<IServerEntityManagerInternal>();
            MapManager = IoCManager.Resolve<IMapManager>();
            MapManager.CreateMap();

            IoCManager.Resolve<ISerializationManager>().Initialize();
            var manager = IoCManager.Resolve<IPrototypeManager>();
            manager.RegisterKind(typeof(EntityPrototype));
            manager.LoadFromStream(new StringReader(PROTOTYPES));
            manager.ResolveResults();

            // build the net dream
            MapA = MapManager.CreateMap();
            GridA = MapManager.CreateGrid(MapA);

            MapB = MapManager.CreateMap();
            GridB = MapManager.CreateGrid(MapB);

            //NOTE: The grids have not moved, so we can assert worldpos == localpos for the test
        }

        [SetUp]
        public void ClearSimulation()
        {
            // One of the tests changes this so we use this to ensure it doesn't get passed to other tests.
            IoCManager.Resolve<IGameTiming>().InSimulation = false;
        }

        [Test]
        public void ParentMapSwitchTest()
        {
            // two entities
            var parent = EntityManager.SpawnEntity("dummy", InitialPos);
            var child = EntityManager.SpawnEntity("dummy", InitialPos);

            var parentTrans = EntityManager.GetComponent<TransformComponent>(parent);
            var childTrans = EntityManager.GetComponent<TransformComponent>(child);

            // that are not on the same map
            parentTrans.Coordinates = new EntityCoordinates(GridA.Owner, (5, 5));
            childTrans.Coordinates = new EntityCoordinates(GridB.Owner, (4, 4));

            // if they are parented, the child keeps its world position, but moves to the parents map
            childTrans.AttachParent(parentTrans);


            Assert.Multiple(() =>
            {
                Assert.That(childTrans.MapID, Is.EqualTo(parentTrans.MapID));
                Assert.That(childTrans.GridUid, Is.EqualTo(parentTrans.GridUid));
                Assert.That(childTrans.Coordinates, Is.EqualTo(new EntityCoordinates(parentTrans.Owner, (-1, -1))));
                Assert.That(childTrans.WorldPosition, Is.EqualTo(new Vector2(4, 4)));
            });

            // move the parent, and the child should move with it
            childTrans.LocalPosition = new Vector2(6, 6);
            parentTrans.WorldPosition = new Vector2(-8, -8);

            Assert.That(childTrans.WorldPosition, Is.EqualTo(new Vector2(-2, -2)));

            // if we detach parent, the child should be left where it was, still relative to parents grid
            var oldLpos = new Vector2(-2, -2);
            var oldWpos = childTrans.WorldPosition;

            childTrans.AttachToGridOrMap();

            // the gridId won't match, because we just detached from the grid entity

            Assert.Multiple(() =>
            {
                Assert.That(childTrans.Coordinates.Position, Is.EqualTo(oldLpos));
                Assert.That(childTrans.WorldPosition, Is.EqualTo(oldWpos));
            });
        }

        /// <summary>
        ///     Tests that a child entity does not move when attaching to a parent.
        /// </summary>
        [Test]
        public void ParentAttachMoveTest()
        {
            // Arrange
            var parent = EntityManager.SpawnEntity("dummy", InitialPos);
            var child = EntityManager.SpawnEntity("dummy", InitialPos);
            var parentTrans = EntityManager.GetComponent<TransformComponent>(parent);
            var childTrans = EntityManager.GetComponent<TransformComponent>(child);
            parentTrans.WorldPosition = new Vector2(5, 5);
            childTrans.WorldPosition = new Vector2(6, 6);

            // Act
            var oldWpos = childTrans.WorldPosition;
            childTrans.AttachParent(parentTrans);
            var newWpos = childTrans.WorldPosition;

            // Assert
            Assert.That(oldWpos == newWpos);
        }

        /// <summary>
        ///     Tests that a child entity does not move when attaching to a parent.
        /// </summary>
        [Test]
        public void ParentDoubleAttachMoveTest()
        {
            // Arrange
            var parent = EntityManager.SpawnEntity("dummy", InitialPos);
            var childOne = EntityManager.SpawnEntity("dummy", InitialPos);
            var childTwo = EntityManager.SpawnEntity("dummy", InitialPos);
            var parentTrans = EntityManager.GetComponent<TransformComponent>(parent);
            var childOneTrans = EntityManager.GetComponent<TransformComponent>(childOne);
            var childTwoTrans = EntityManager.GetComponent<TransformComponent>(childTwo);
            parentTrans.WorldPosition = new Vector2(1, 1);
            childOneTrans.WorldPosition = new Vector2(2, 2);
            childTwoTrans.WorldPosition = new Vector2(3, 3);

            // Act
            var oldWpos = childOneTrans.WorldPosition;
            childOneTrans.AttachParent(parentTrans);
            var newWpos = childOneTrans.WorldPosition;
            Assert.That(oldWpos, Is.EqualTo(newWpos));

            oldWpos = childTwoTrans.WorldPosition;
            childTwoTrans.AttachParent(parentTrans);
            newWpos = childTwoTrans.WorldPosition;
            Assert.That(oldWpos, Is.EqualTo(newWpos));

            oldWpos = childTwoTrans.WorldPosition;
            childTwoTrans.AttachParent(childOneTrans);
            newWpos = childTwoTrans.WorldPosition;
            Assert.That(oldWpos, Is.EqualTo(newWpos));
        }

        /// <summary>
        ///     Tests that the entity orbits properly when the parent rotates.
        /// </summary>
        [Test]
        public void ParentRotateTest()
        {
            // Arrange
            var parent = EntityManager.SpawnEntity("dummy", InitialPos);
            var child = EntityManager.SpawnEntity("dummy", InitialPos);
            var parentTrans = EntityManager.GetComponent<TransformComponent>(parent);
            var childTrans = EntityManager.GetComponent<TransformComponent>(child);
            parentTrans.WorldPosition = new Vector2(0, 0);
            childTrans.WorldPosition = new Vector2(2, 0);
            childTrans.AttachParent(parentTrans);

            //Act
            parentTrans.LocalRotation = new Angle(MathHelper.Pi / 2);

            //Assert
            var result = childTrans.WorldPosition;
            Assert.Multiple(() =>
            {
                Assert.That(MathHelper.CloseToPercent(result.X, 0));
                Assert.That(MathHelper.CloseToPercent(result.Y, 2));
            });
        }

        /// <summary>
        ///     Tests that the entity orbits properly when the parent rotates and is not at the origin.
        /// </summary>
        [Test]
        public void ParentTransRotateTest()
        {
            // Arrange
            var parent = EntityManager.SpawnEntity("dummy", InitialPos);
            var child = EntityManager.SpawnEntity("dummy", InitialPos);
            var parentTrans = EntityManager.GetComponent<TransformComponent>(parent);
            var childTrans = EntityManager.GetComponent<TransformComponent>(child);
            parentTrans.WorldPosition = new Vector2(1, 1);
            childTrans.WorldPosition = new Vector2(2, 1);
            childTrans.AttachParent(parentTrans);

            //Act
            parentTrans.LocalRotation = new Angle(MathHelper.Pi / 2);

            //Assert
            var result = childTrans.WorldPosition;
            Assert.Multiple(() =>
            {
                Assert.That(MathHelper.CloseToPercent(result.X, 1));
                Assert.That(MathHelper.CloseToPercent(result.Y, 2));
            });
        }

        /// <summary>
        ///     Tests to see if parenting multiple entities with WorldPosition places the leaf properly.
        /// </summary>
        [Test]
        public void PositionCompositionTest()
        {
            // Arrange
            var node1 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node2 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node3 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node4 = EntityManager.SpawnEntity("dummy", InitialPos);

            var node1Trans = EntityManager.GetComponent<TransformComponent>(node1);
            var node2Trans = EntityManager.GetComponent<TransformComponent>(node2);
            var node3Trans = EntityManager.GetComponent<TransformComponent>(node3);
            var node4Trans = EntityManager.GetComponent<TransformComponent>(node4);

            node1Trans.WorldPosition = new Vector2(0, 0);
            node2Trans.WorldPosition = new Vector2(1, 1);
            node3Trans.WorldPosition = new Vector2(2, 2);
            node4Trans.WorldPosition = new Vector2(0, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);
            node4Trans.AttachParent(node3Trans);

            //Act
            node1Trans.LocalRotation = new Angle(MathHelper.Pi / 2);

            //Assert
            var result = node4Trans.WorldPosition;

            Assert.Multiple(() =>
            {
                Assert.That(result.X, new ApproxEqualityConstraint(-2f));
                Assert.That(result.Y, new ApproxEqualityConstraint(0f));
            });
        }

        /// <summary>
        ///     Tests to see if setting the world position of a child causes position rounding errors.
        /// </summary>
        [Test]
        public void ParentLocalPositionRoundingErrorTest()
        {
            // Arrange
            var node1 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node2 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node3 = EntityManager.SpawnEntity("dummy", InitialPos);

            var node1Trans = EntityManager.GetComponent<TransformComponent>(node1);
            var node2Trans = EntityManager.GetComponent<TransformComponent>(node2);
            var node3Trans = EntityManager.GetComponent<TransformComponent>(node3);

            node1Trans.WorldPosition = new Vector2(0, 0);
            node2Trans.WorldPosition = new Vector2(1, 1);
            node3Trans.WorldPosition = new Vector2(2, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);

            // Act
            var oldWpos = node3Trans.WorldPosition;

            for (var i = 0; i < 10000; i++)
            {
                var dx = i % 2 == 0 ? 5 : -5;
                node1Trans.LocalPosition += new Vector2(dx, dx);
                node2Trans.LocalPosition += new Vector2(dx, dx);
                node3Trans.LocalPosition += new Vector2(dx, dx);
            }

            var newWpos = node3Trans.WorldPosition;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(MathHelper.CloseToPercent(oldWpos.X, newWpos.Y), $"{oldWpos.X} should be {newWpos.Y}");
                Assert.That(MathHelper.CloseToPercent(oldWpos.Y, newWpos.Y), newWpos.ToString);
            });
        }

        /// <summary>
        ///     Tests to see if rotating a parent causes major child position rounding errors.
        /// </summary>
        [Test]
        public void ParentRotationRoundingErrorTest()
        {
            IoCManager.Resolve<IGameTiming>().InSimulation = true;

            // Arrange
            var node1 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node2 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node3 = EntityManager.SpawnEntity("dummy", InitialPos);

            var node1Trans = EntityManager.GetComponent<TransformComponent>(node1);
            var node2Trans = EntityManager.GetComponent<TransformComponent>(node2);
            var node3Trans = EntityManager.GetComponent<TransformComponent>(node3);

            node1Trans.WorldPosition = new Vector2(0, 0);
            node2Trans.WorldPosition = new Vector2(1, 1);
            node3Trans.WorldPosition = new Vector2(2, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);

            // Act
            var oldWpos = node3Trans.WorldPosition;

            for (var i = 0; i < 100; i++)
            {
                node1Trans.LocalRotation += new Angle(MathHelper.Pi);
                node2Trans.LocalRotation += new Angle(MathHelper.Pi);
                node3Trans.LocalRotation += new Angle(MathHelper.Pi);
            }

            var newWpos = node3Trans.WorldPosition;

            //NOTE: Yes, this does cause a non-zero error

            // Assert

            Assert.Multiple(() =>
            {
                Assert.That(MathHelper.CloseToPercent(oldWpos.X, newWpos.Y));
                Assert.That(MathHelper.CloseToPercent(oldWpos.Y, newWpos.Y));
            });
        }

        /// <summary>
        ///     Tests that the world and inverse world transforms are built properly.
        /// </summary>
        [Test]
        public void TreeComposeWorldMatricesTest()
        {
            // Arrange
            var control = Matrix3.Identity;

            var node1 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node2 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node3 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node4 = EntityManager.SpawnEntity("dummy", InitialPos);

            var node1Trans = EntityManager.GetComponent<TransformComponent>(node1);
            var node2Trans = EntityManager.GetComponent<TransformComponent>(node2);
            var node3Trans = EntityManager.GetComponent<TransformComponent>(node3);
            var node4Trans = EntityManager.GetComponent<TransformComponent>(node4);

            node1Trans.WorldPosition = new Vector2(0, 0);
            node2Trans.WorldPosition = new Vector2(1, 1);
            node3Trans.WorldPosition = new Vector2(2, 2);
            node4Trans.WorldPosition = new Vector2(0, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);
            node4Trans.AttachParent(node3Trans);

            //Act
            node1Trans.LocalRotation = new Angle(MathHelper.Pi / 6.37);
            node1Trans.WorldPosition = new Vector2(1, 1);

            var worldMat = node4Trans.WorldMatrix;
            var invWorldMat = node4Trans.InvWorldMatrix;

            Matrix3.Multiply(in worldMat, in invWorldMat, out var leftVerifyMatrix);
            Matrix3.Multiply(in invWorldMat, in worldMat, out var rightVerifyMatrix);

            //Assert

            Assert.Multiple(() =>
            {
                // these should be the same (A × A-1 = A-1 × A = I)
                Assert.That(leftVerifyMatrix, new ApproxEqualityConstraint(rightVerifyMatrix));

                // verify matrix == identity matrix (or very close to because float precision)
                Assert.That(leftVerifyMatrix, new ApproxEqualityConstraint(control));
            });
        }

        /// <summary>
        ///     Tests that world rotation is built properly
        /// </summary>
        [Test]
        public void WorldRotationTest()
        {
            // Arrange
            var node1 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node2 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node3 = EntityManager.SpawnEntity("dummy", InitialPos);

            var node1Trans = EntityManager.GetComponent<TransformComponent>(node1);
            var node2Trans = EntityManager.GetComponent<TransformComponent>(node2);
            var node3Trans = EntityManager.GetComponent<TransformComponent>(node3);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);

            node1Trans.LocalRotation = Angle.FromDegrees(0);
            node2Trans.LocalRotation = Angle.FromDegrees(45);
            node3Trans.LocalRotation = Angle.FromDegrees(45);

            // Act
            node1Trans.LocalRotation = Angle.FromDegrees(135);

            // Assert (135 + 45 + 45 = 225)
            var result = node3Trans.WorldRotation;
            Assert.That(result, new ApproxEqualityConstraint(Angle.FromDegrees(225)));
        }

        /// <summary>
        ///     Test that, in a chain A -> B -> C, if A is moved C's world position correctly updates.
        /// </summary>
        [Test]
        public void MatrixUpdateTest()
        {
            var node1 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node2 = EntityManager.SpawnEntity("dummy", InitialPos);
            var node3 = EntityManager.SpawnEntity("dummy", InitialPos);

            var node1Trans = EntityManager.GetComponent<TransformComponent>(node1);
            var node2Trans = EntityManager.GetComponent<TransformComponent>(node2);
            var node3Trans = EntityManager.GetComponent<TransformComponent>(node3);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);

            node3Trans.LocalPosition = new Vector2(5, 5);
            node2Trans.LocalPosition = new Vector2(5, 5);
            node1Trans.LocalPosition = new Vector2(5, 5);

            Assert.That(node3Trans.WorldPosition, new ApproxEqualityConstraint(new Vector2(15, 15)));
        }

        /*
         * There used to be a TestMapInitOrder test here. The problem is that the actual game will probably explode if
         * you start initialising children before parents and the test only worked because of specific setup being done
         * to prevent this in its use case.
         */
    }
}
