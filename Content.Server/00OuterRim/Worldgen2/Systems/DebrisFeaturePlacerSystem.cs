using Content.Server._00OuterRim.Worldgen.Tools;
using Content.Server._00OuterRim.Worldgen2.Components;
using Robust.Shared.Map;

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

        var points = GeneratePointsInChunk(args.Chunk, density, xform);

        foreach (var point in points)
        {
            Spawn("ORDummy", new EntityCoordinates(uid, point));
        }
    }

    private List<Vector2> GeneratePointsInChunk(EntityUid chunk, float density, TransformComponent? xform = null)
    {
        if (!Resolve(chunk, ref xform))
            throw new Exception("Failed to resolve transform, somehow.");

        var coords = GetChunkCoords(chunk, xform);

        var offs = (int)((WorldGen.ChunkSize - (density / 2)) / 2);

        var topLeft = (-offs, -offs);
        var lowerRight = (offs, offs);
        Logger.Debug($"{offs}, {topLeft}, {lowerRight}. {density}");
        var debrisPoints = _sampler.SampleRectangle(topLeft, lowerRight, density);

        for (var i = 0; i < debrisPoints.Count; i++)
        {
            debrisPoints[i] += xform.WorldPosition;
        }

        return debrisPoints;
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
