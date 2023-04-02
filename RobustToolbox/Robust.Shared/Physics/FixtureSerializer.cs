using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Robust.Shared.Physics;

/// <summary>
/// Special-case to avoid writing grid fixtures.
/// </summary>
public sealed class FixtureSerializer : ITypeSerializer<List<Fixture>, SequenceDataNode>
{
    public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
        IDependencyCollection dependencies, ISerializationContext? context = null)
    {
        var seq = new List<ValidationNode>(node.Count);

        foreach (var subNode in node)
        {
            seq.Add(serializationManager.ValidateNode<Fixture>(subNode, context));
        }

        return new ValidatedSequenceNode(seq);
    }

    public List<Fixture> Read(ISerializationManager serializationManager, SequenceDataNode node, IDependencyCollection dependencies,
        SerializationHookContext hookCtx, ISerializationContext? context = null, ISerializationManager.InstantiationDelegate<List<Fixture>>? instantiation = default)
    {
        var value = instantiation != null ? instantiation() : new List<Fixture>(node.Count);

        foreach (var subNode in node)
        {
            var fixture = serializationManager.Read<Fixture>(subNode, hookCtx, context, notNullableOverride: true);
            value.Add(fixture);
        }

        return value;
    }

    public DataNode Write(ISerializationManager serializationManager, List<Fixture> value, IDependencyCollection dependencies,
        bool alwaysWrite = false, ISerializationContext? context = null)
    {
        var seq = new SequenceDataNode();

        if (value.Count == 0)
            return seq;

        if (context is MapSerializationContext mapContext)
        {
            // Don't serialize mapgrid fixtures because it's bloat and we'll just generate them at runtime.
            if (dependencies.Resolve<IEntityManager>().HasComponent<MapGridComponent>(mapContext.CurrentWritingEntity))
            {
                return seq;
            }
        }

        foreach (var fixture in value)
        {
            seq.Add(serializationManager.WriteValue(fixture, alwaysWrite, context, notNullableOverride: true));
        }

        return seq;
    }
}
