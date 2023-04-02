using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Robust.Client.UserInterface.RichText;

[Prototype("font")]
public sealed class FontPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("path", required: true)]
    public ResourcePath Path { get; } = default!;
}
