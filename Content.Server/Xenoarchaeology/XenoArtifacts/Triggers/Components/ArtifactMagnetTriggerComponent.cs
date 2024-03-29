﻿namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
/// Triggers when the salvage magnet is activated
/// </summary>
[RegisterComponent]
public sealed class ArtifactMagnetTriggerComponent : Component
{
    /// <summary>
    /// how close to the magnet do you have to be?
    /// </summary>
    [DataField("range")]
    public float Range = 40f;
}
