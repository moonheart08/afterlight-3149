using Content.Server._00OuterRim.Worldgen2.Prototypes;
using Content.Server._00OuterRim.Worldgen2.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._00OuterRim.Worldgen2.Components;

/// <summary>
/// This is used for controlling the debris feature placer.
/// </summary>
[RegisterComponent, Access(typeof(DebrisFeaturePlacerSystem))]
public sealed class DebrisFeaturePlacerControllerComponent : Component
{
    [DataField("densityNoiseChannel", customTypeSerializer: typeof(PrototypeIdSerializer<NoiseChannelPrototype>))]
    public string DensityNoiseChannel { get; }= default!;

    public List<EntityUid> OwnedDebris = new();

    public bool DoSpawns = true;
}
