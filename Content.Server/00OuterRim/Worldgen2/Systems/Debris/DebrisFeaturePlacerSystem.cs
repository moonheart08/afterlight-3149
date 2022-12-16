using Content.Server._00OuterRim.Worldgen2.Components;
using Content.Server._00OuterRim.Worldgen2.Tools;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._00OuterRim.Worldgen2.Systems.Debris;

/// <summary>
/// This handles...
/// </summary>
public sealed class DebrisFeaturePlacerSystem : BaseWorldSystem
{
    [Dependency] private readonly DeferredSpawnSystem _deferred = default!;
    [Dependency] private readonly NoiseIndexSystem _noiseIndex = default!;
    [Dependency] private readonly PoissonDiskSampler _sampler = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private ISawmill _sawmill = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("world.debris.feature_placer");
        SubscribeLocalEvent<DebrisFeaturePlacerControllerComponent, WorldChunkLoadedEvent>(OnChunkLoaded);
        SubscribeLocalEvent<SimpleDebrisSelectorComponent, TryGetPlaceableDebrisFeatureEvent>(OnTryGetPlacableDebrisEvent);
        SubscribeLocalEvent<TieDebrisToFeaturePlacerEvent>(OnDeferredDone);
    }

    private void OnTryGetPlacableDebrisEvent(EntityUid uid, SimpleDebrisSelectorComponent component, TryGetPlaceableDebrisFeatureEvent args)
    {
        if (args.DebrisProto is not null)
            return;

        var l = new List<string?>(1);
        component.CachedDebrisTable.GetSpawns(_random, ref l);

        switch (l.Count)
        {
            case 0:
                return;
            case > 1:
                _sawmill.Warning($"Got more than one possible debris type from {uid}. List: {string.Join(", ", l)}");
                break;
        }

        args.DebrisProto = l[0];
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

        var failures = 0; // Avoid severe log spam.
        foreach (var point in points)
        {
            var pointDensity = _noiseIndex.Evaluate(uid, densityChannel, WorldGen.WorldToChunkCoords(point));
            if (pointDensity == 0 || _random.Prob(component.RandomCancellationChance))
                continue;

            var coords = new EntityCoordinates(uid, point);

            var preEv = new PrePlaceDebrisFeatureEvent(coords, args.Chunk);
            RaiseLocalEvent(uid, ref preEv);
            if (uid != args.Chunk)
                RaiseLocalEvent(args.Chunk, ref preEv);

            if (preEv.Cancelled)
                continue;

            var debrisFeatureEv = new TryGetPlaceableDebrisFeatureEvent(coords, args.Chunk);
            RaiseLocalEvent(uid, ref debrisFeatureEv);

            if (debrisFeatureEv.DebrisProto == null)
            {
                // Try on the chunk...?
                if (uid != args.Chunk)
                    RaiseLocalEvent(args.Chunk, ref debrisFeatureEv);

                if (debrisFeatureEv.DebrisProto == null)
                {
                    // Nope.
                    failures++;
                    continue;
                }
            }

            _deferred.SpawnEntityDeferred(debrisFeatureEv.DebrisProto, coords, new TieDebrisToFeaturePlacerEvent(args.Chunk));
        }

        if (failures > 0)
            _sawmill.Error($"Failed to place {failures} debris at chunk {args.Chunk}");

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

/// <summary>
/// Fired on the debris feature placer controller and the chunk, ahead of placing a debris piece.
/// </summary>
[ByRefEvent]
public record struct PrePlaceDebrisFeatureEvent(EntityCoordinates Coords, EntityUid Chunk, bool Cancelled = false);

public sealed class TieDebrisToFeaturePlacerEvent : DeferredSpawnDoneEvent
{
    public EntityUid DebrisPlacer;

    public TieDebrisToFeaturePlacerEvent(EntityUid debrisPlacer)
    {
        DebrisPlacer = debrisPlacer;
    }
}

[ByRefEvent]
public record struct TryGetPlaceableDebrisFeatureEvent(EntityCoordinates Coords, EntityUid Chunk, string? DebrisProto = null);
