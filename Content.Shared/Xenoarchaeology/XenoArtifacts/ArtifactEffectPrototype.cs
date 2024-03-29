﻿using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Xenoarchaeology.XenoArtifacts;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("artifactEffect")]
[DataDefinition]
public sealed class ArtifactEffectPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Components that are added to the artifact when the specfic effect is active.
    /// These are removed after the node is exited and the effect is changed.
    /// </summary>
    [DataField("components", serverOnly: true)]
    public EntityPrototype.ComponentRegistry Components = new();

    /// <summary>
    /// Components that are permanently added to an entity when the effect's node is entered.
    /// </summary>
    [DataField("permanentComponents")]
    public EntityPrototype.ComponentRegistry PermanentComponents = new();

    //TODO: make this a list so we can have multiple target depths
    [DataField("targetDepth")]
    public int TargetDepth = 0;

    [DataField("effectHint")]
    public string? EffectHint;
}
