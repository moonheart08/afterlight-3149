using Content.Server._00OuterRim.Worldgen2.Prototypes;
using Content.Server.GameTicking;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._00OuterRim.Worldgen2.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class WorldgenConfigSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISerializationManager _ser = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private bool _enabled = false;
    private string _worldgenConfig = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LoadingMapsEvent>(OnLoadingMaps);
        _cfg.OnValueChanged(WorldgenCVars.WorldgenEnabled, b => _enabled = b, true);
        _cfg.OnValueChanged(WorldgenCVars.WorldgenConfig, s => _worldgenConfig = s, true);
    }

    private void OnLoadingMaps(LoadingMapsEvent ev)
    {
        if (_enabled == false)
            return;

        var target = _map.GetMapEntityId(_gameTicker.DefaultMap);
        var cfg = _proto.Index<WorldgenConfigPrototype>(_worldgenConfig);

        cfg.Apply(target, _ser, EntityManager); // Apply the config to the map.
    }
}
