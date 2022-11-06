using Robust.Shared.Configuration;

namespace Content.Server._00OuterRim.Worldgen2;

[CVarDefs]
public sealed class WorldgenCVars
{
    /// <summary>
    /// Whether or not world generation is enabled.
    /// </summary>
    public static readonly CVarDef<bool> WorldgenEnabled =
        CVarDef.Create("al3149.worldgen.enabled", false, CVar.SERVERONLY);

    /// <summary>
    /// The worldgen config to use.
    /// </summary>
    public static readonly CVarDef<string> WorldgenConfig =
        CVarDef.Create("al3149.worldgen.worldgen_config", "Default", CVar.SERVERONLY);
}
