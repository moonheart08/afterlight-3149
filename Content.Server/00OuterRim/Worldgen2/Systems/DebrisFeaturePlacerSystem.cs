using Content.Server._00OuterRim.Worldgen.Tools;
using Content.Server._00OuterRim.Worldgen2.Components;
using Robust.Shared.Map;

namespace Content.Server._00OuterRim.Worldgen2.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class DebrisFeaturePlacerSystem : BaseWorldSystem
{
    [Dependency] private readonly DeferredSpawnSystem _deferred = default!;
    [Dependency] private readonly NoiseIndexSystem _noiseIndex = default!;
    [Dependency] private readonly PoissonDiskSampler _sampler = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DebrisFeaturePlacerControllerComponent, WorldChunkLoadedEvent>(OnChunkLoaded);
        SubscribeLocalEvent<TieDebrisToFeaturePlacerEvent>(OnDeferredDone);
    }

    private void OnDeferredDone(TieDebrisToFeaturePlacerEvent ev)
    {
        var placer = Comp<DebrisFeaturePlacerControllerComponent>(ev.DebrisPlacer);
        placer.OwnedDebris.Add(ev.SpawnedEntity);
    }

    private void OnChunkLoaded(EntityUid uid, DebrisFeaturePlacerControllerComponent component, ref WorldChunkLoadedEvent args)
    {
        if (component.DoSpawns == false)
            return;

        component.DoSpawns = false; // Don't repeat yourself if this crashes.

        var densityChannel = component.DensityNoiseChannel;
        var xform = Transform(args.Chunk);
        var density = _noiseIndex.Evaluate(uid, densityChannel, GetFloatingChunkCoords(args.Chunk, xform) + new Vector2(0.5f, 0.5f));
        if (density == 0)
            return;

        var points = GeneratePointsInChunk(args.Chunk, density, xform);

        foreach (var point in points)
        {
            var pointDensity = _noiseIndex.Evaluate(uid, densityChannel, WorldGen.WorldToChunkCoords(point));
            if (pointDensity == 0)
                continue;
            _deferred.SpawnEntityDeferred("ORDummy", new EntityCoordinates(uid, point), new TieDebrisToFeaturePlacerEvent(args.Chunk));
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
        var debrisPoints = _sampler.SampleRectangle(topLeft, lowerRight, density);


        for (var i = 0; i < debrisPoints.Count; i++)
        {
            debrisPoints[i] += WorldGen.ChunkToWorldCoordsCentered(coords);
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

public sealed class TieDebrisToFeaturePlacerEvent : DeferredSpawnDoneEvent
{
    public EntityUid DebrisPlacer;

    public TieDebrisToFeaturePlacerEvent(EntityUid debrisPlacer)
    {
        DebrisPlacer = debrisPlacer;
    }
}
