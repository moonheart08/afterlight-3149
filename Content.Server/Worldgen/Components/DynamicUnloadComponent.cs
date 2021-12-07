using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Worldgen.Components;

/// <summary>
/// Component used by points of interest
/// </summary>
public class DynamicUnloadComponent : Component
{
    public override string Name => "DynamicUnload";

    // Used for points of interest before banishing them to map 1
    [DataField("maximumViewerDistance")]
    public float MaximumViewerDistance = 512f;
}
