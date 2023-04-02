using System;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;

namespace Robust.Server.Console.Commands;

public sealed class ScaleCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override string Command => "scale";
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError($"Insufficient number of args supplied: expected 2 and received {args.Length}");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError($"Unable to find entity {args[0]}");
            return;
        }

        if (!float.TryParse(args[1], out var scale))
        {
            shell.WriteError($"Invalid scale supplied of {args[0]}");
            return;
        }

        if (scale < 0f)
        {
            shell.WriteError($"Invalid scale supplied that is negative!");
            return;
        }

        // Event for content to use
        // We'll just set engine stuff here
        var physics = _entityManager.System<SharedPhysicsSystem>();
        var appearance = _entityManager.System<AppearanceSystem>();

        _entityManager.EnsureComponent<ScaleVisualsComponent>(uid);
        var @event = new ScaleEntityEvent();
        _entityManager.EventBus.RaiseLocalEvent(uid, ref @event);

        var appearanceComponent = _entityManager.EnsureComponent<ServerAppearanceComponent>(uid);
        if (!appearance.TryGetData<Vector2>(uid, ScaleVisuals.Scale, out var oldScale, appearanceComponent))
            oldScale = Vector2.One;

        appearance.SetData(uid, ScaleVisuals.Scale, oldScale * scale, appearanceComponent);

        if (_entityManager.TryGetComponent(uid, out FixturesComponent? manager))
        {
            foreach (var fixture in manager.Fixtures.Values)
            {
                switch (fixture.Shape)
                {
                    case EdgeShape edge:
                        physics.SetVertices(uid, fixture, edge,
                            edge.Vertex0 * scale,
                            edge.Vertex1 * scale,
                            edge.Vertex2 * scale,
                            edge.Vertex3 * scale, manager);
                        break;
                    case PhysShapeCircle circle:
                        physics.SetPositionRadius(uid, fixture, circle, circle.Position * scale, circle.Radius * scale, manager);
                        break;
                    case PolygonShape poly:
                        var verts = poly.Vertices;

                        for (var i = 0; i < verts.Length; i++)
                        {
                            verts[i] *= scale;
                        }

                        physics.SetVertices(uid, fixture, poly, verts, manager);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    [ByRefEvent]
    public readonly record struct ScaleEntityEvent(EntityUid Uid) {}
}
