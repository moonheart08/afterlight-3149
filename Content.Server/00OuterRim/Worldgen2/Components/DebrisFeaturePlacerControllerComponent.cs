﻿using Content.Server._00OuterRim.Worldgen2.Prototypes;
using Content.Server._00OuterRim.Worldgen2.Systems;
using Content.Server._00OuterRim.Worldgen2.Systems.Debris;
using Content.Shared.Storage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._00OuterRim.Worldgen2.Components;

/// <summary>
/// This is used for controlling the debris feature placer.
/// </summary>
[RegisterComponent, Access(typeof(DebrisFeaturePlacerSystem))]
public sealed class DebrisFeaturePlacerControllerComponent : Component
{
    /// <summary>
    /// The noise channel to use as a density controller.
    /// </summary>
    [DataField("densityNoiseChannel", customTypeSerializer: typeof(PrototypeIdSerializer<NoiseChannelPrototype>))]
    public string DensityNoiseChannel { get; }= default!;

    /// <summary>
    /// Whether or not to clip debris that would spawn at a location that has a density of zero.
    /// </summary>
    [DataField("densityClip")]
    public bool DensityClip = true;

    /// <summary>
    /// The chance spawning a piece of debris will just be cancelled randomly.
    /// </summary>
    [DataField("randomCancelChance")]
    public float RandomCancellationChance = 0.1f;

    public List<EntityUid> OwnedDebris = new();

    public bool DoSpawns = true;


}
