using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server._00OuterRim.Worldgen.v2;

/// <summary>
/// It's a secret to everyone.
/// To use this, do NOT depend on it! Instead use IEntitySystemManager.GetEntitySystemOrNull, as it may not exist.
/// </summary>
/// <remarks>
/// If an implementation of this system is present, default world generation is DISABLED.
/// </remarks>
public abstract class PublicSecretWorldgenSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager ConfigurationManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        ConfigurationManager.OverrideDefault(CCVars.WorldGenEnabled, false);
    }
}
