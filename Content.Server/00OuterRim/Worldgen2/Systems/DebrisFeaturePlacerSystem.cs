using Content.Server._00OuterRim.Worldgen.Tools;
using Content.Server._00OuterRim.Worldgen2.Components;

namespace Content.Server._00OuterRim.Worldgen2.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class DebrisFeaturePlacerSystem : BaseWorldSystem
{
    [Dependency] private readonly NoiseIndexSystem _noiseIndex = default!;
    [Dependency] private readonly PoissonDiskSampler _sampler = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DebrisFeaturePlacerControllerComponent, WorldChunkLoadedEvent>(OnChunkLoaded);
    }

    private void OnChunkLoaded(EntityUid uid, DebrisFeaturePlacerControllerComponent component, WorldChunkLoadedEvent args)
    {
        var densityChannel = component.DensityNoiseChannel;
        var xform = Transform(args.Chunk);
        var density = _noiseIndex.Evaluate(uid, densityChannel, GetChunkCoords(args.Chunk, xform));
        if (density == 0)
            return;
    }

    private List<Vector2> GeneratePointsInChunk(EntityUid chunk, TransformComponent? xform = null)
    {
        if (!Resolve(chunk, ref xform))
            throw new Exception("Failed to resolve transform, somehow.");

        var coords = GetChunkCoords(chunk, xform);

        var centerCoords = WorldGen.ChunkToWorldCoordsCentered(coords);
    }
}

[ByRefEvent]
public struct PrePlaceDebrisFeatureEvent
{
    public bool Cancelled = false;

    public PrePlaceDebrisFeatureEvent() { }
}

[ByRefEvent]
public struct PostPlaceDebrisFeatureEvent
{
    public IReadOnlyList<EntityUid> PlacedFeatures;

    public PostPlaceDebrisFeatureEvent(IReadOnlyList<EntityUid> placedFeatures)
    {
        PlacedFeatures = placedFeatures;
    }
}
