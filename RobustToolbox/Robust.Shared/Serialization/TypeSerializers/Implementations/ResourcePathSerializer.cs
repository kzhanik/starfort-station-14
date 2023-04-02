using System;
using System.Linq;
using JetBrains.Annotations;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using Robust.Shared.Utility;

namespace Robust.Shared.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public sealed class ResourcePathSerializer : ITypeSerializer<ResourcePath, ValueDataNode>, ITypeCopyCreator<ResourcePath>
    {
        public ResourcePath Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            SerializationHookContext hookCtx,
            ISerializationContext? context = null,
            ISerializationManager.InstantiationDelegate<ResourcePath>? instanceProvider = null)
        {
            return new ResourcePath(node.Value);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var path = new ResourcePath(node.Value);

            if (path.Extension.Equals("rsi"))
            {
                path /= "meta.json";
            }

            if (!path.EnumerateSegments().First().Equals("Textures", StringComparison.InvariantCultureIgnoreCase))
            {
                path = SharedSpriteComponent.TextureRoot / path;
            }

            path = path.ToRootedPath();


            try
            {
                var resourceManager = dependencies.Resolve<IResourceManager>();
                if(resourceManager.ContentFileExists(path))
                {
                    return new ValidatedValueNode(node);
                }

                if (node.Value.EndsWith(path.Separator) && resourceManager.ContentGetDirectoryEntries(path).Any())
                {
                    return new ValidatedValueNode(node);
                }

                return new ErrorNode(node, $"File not found. ({path})");
            }
            catch (Exception e)
            {
                return new ErrorNode(node, $"Failed parsing filepath. ({path}) ({e.Message})");
            }
        }

        public DataNode Write(ISerializationManager serializationManager, ResourcePath value,
            IDependencyCollection dependencies,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString());
        }

        [MustUseReturnValue]
        public ResourcePath CreateCopy(ISerializationManager serializationManager, ResourcePath source,
            SerializationHookContext hookCtx,
            ISerializationContext? context = null)
        {
            return new(source.ToString());
        }
    }
}
