using Content.Server._00OuterRim.Worldgen2.Components;
using Content.Server.Ghost.Components;
using Content.Server.Mind.Components;
using Robust.Shared.Map;

namespace Content.Server._00OuterRim.Worldgen2.Systems;

/// <summary>
/// This handles putting together chunk entities and notifying them about important changes.
/// </summary>
public sealed class WorldControllerSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("world");
        SubscribeLocalEvent<WorldChunkComponent, ComponentStartup>(OnChunkStartup);
        SubscribeLocalEvent<LoadedChunkComponent, ComponentStartup>(OnChunkLoadedCore);
        SubscribeLocalEvent<LoadedChunkComponent, ComponentShutdown>(OnChunkUnloadedCore);
        Logger.Debug($"The chunk 0,0 has center 64, 64, which is chunk {WorldGen.WorldToChunkCoords(new Vector2i(64, 64))}");
    }

    /// <summary>
    /// Handles the inner logic of loading a chunk, i.e. events.
    /// </summary>
    private void OnChunkLoadedCore(EntityUid uid, LoadedChunkComponent component, ComponentStartup args)
    {
        var xform = Transform(uid);
        if (xform.MapUid is null)
            return;
        var coords = WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));
        var ev = new WorldChunkLoadedEvent(uid, coords);
        RaiseLocalEvent(xform.MapUid.Value, ev);
        RaiseLocalEvent(uid, ev);
        _sawmill.Debug($"Loaded chunk {ToPrettyString(uid)} at {coords}");
    }

    /// <summary>
    /// Handles the inner logic of unloading a chunk, i.e. events.
    /// </summary>
    private void OnChunkUnloadedCore(EntityUid uid, LoadedChunkComponent component, ComponentShutdown args)
    {
        var xform = Transform(uid);
        if (xform.MapUid is null)
            return;
        var coords = WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));
        var ev = new WorldChunkUnloadedEvent(uid, coords);
        RaiseLocalEvent(xform.MapUid.Value, ev);
        RaiseLocalEvent(uid, ev);
        _sawmill.Debug($"Unloaded chunk {ToPrettyString(uid)} at {coords}");
    }

    /// <summary>
    /// During setup, make sure the chunk gets attached to the WorldControllerComponent on the map.
    /// </summary>
    private void OnChunkStartup(EntityUid uid, WorldChunkComponent component, ComponentStartup args)
    {
        var xform = Transform(uid);
        var parent = xform.Coordinates.EntityId;

        if (!TryComp<WorldControllerComponent>(parent, out var worldController))
        {
            Logger.Error($"Badly initialized chunk {ToPrettyString(uid)}.");
            return;
        }

        var coords = WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));
        ref var chunks = ref worldController.Chunks;

        if (chunks.ContainsKey(coords))
        {
            Logger.Error($"Tried to initialize chunk {ToPrettyString(uid)} on top of existing chunk {ToPrettyString(chunks[coords])}.");
            return;
        }

        chunks[coords] = uid; // Add this entity to chunk index.

        RaiseLocalEvent(parent, new WorldChunkAddedEvent(uid, coords));
    }

    private const int PlayerLoadRadius = 1;

    /// <summary>
    /// Handles various tasks core to world generation, like chunk loading.
    /// </summary>
    /// <param name="frameTime"></param>
    public override void Update(float frameTime)
    {
        //TODO: Use struct enumerator for all entity queries here once available.
        //TODO: Maybe don't allocate a big collection every frame?
        var chunksToLoad = new Dictionary<EntityUid, HashSet<Vector2i>>();

        foreach (var controller in EntityQuery<WorldControllerComponent>())
        {
            chunksToLoad[controller.Owner] = new();

            List<Vector2i>? chunksToRemove = null;
            foreach (var (idx, chunk) in controller.Chunks)
            {
                if (!Deleted(chunk) && !Terminating(chunk))
                    continue;

                chunksToRemove ??= new(8);
                chunksToRemove.Add(idx);
            }

            if (chunksToRemove is not null)
            {
                foreach (var chunk in chunksToRemove)
                {
                    controller.Chunks.Remove(chunk);
                }
            }
        }

        foreach (var (worldLoader, xform) in EntityQuery<WorldLoaderComponent, TransformComponent>())
        {
            var mapOrNull = xform.MapUid;
            if (mapOrNull is null)
                continue;
            var map = mapOrNull.Value;
            if (!chunksToLoad.ContainsKey(map))
                continue;

            var coords = WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));
            var chunks = new GridPointsNearEnumerator(coords, (int)Math.Ceiling(worldLoader.Radius / 128.0f));

            var set = chunksToLoad[map];

            while (chunks.MoveNext(out var chunk))
            {
                set.Add(chunk.Value);
            }
        }

        // Mindful entities get special privilege as they're always a player and we don't want the illusion being broken around them.
        foreach (var (mind, xform) in EntityQuery<MindComponent, TransformComponent>())
        {
            if (!mind.HasMind)
                continue;
            if (HasComp<GhostComponent>(mind.Owner))
                continue;
            var mapOrNull = xform.MapUid;
            if (mapOrNull is null)
                continue;
            var map = mapOrNull.Value;
            if (!chunksToLoad.ContainsKey(map))
                continue;

            var coords = WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));
            var chunks = new GridPointsNearEnumerator(coords, PlayerLoadRadius);

            var set = chunksToLoad[map];

            while (chunks.MoveNext(out var chunk))
            {
                set.Add(chunk.Value);
            }
        }

        // Make sure these chunks get unloaded at the end of the tick.
        foreach (var (_, xform) in EntityQuery<LoadedChunkComponent, TransformComponent>())
        {
            var mapOrNull = xform.MapUid;
            if (mapOrNull is null)
            {
                RemCompDeferred<LoadedChunkComponent>(xform.Owner);
                continue;
            }

            var map = mapOrNull.Value;
            if (!chunksToLoad.ContainsKey(map))
            {
                RemCompDeferred<LoadedChunkComponent>(xform.Owner);
                continue;
            }

            var coords = WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));

            if (!chunksToLoad[map].Contains(coords))
                RemCompDeferred<LoadedChunkComponent>(xform.Owner);
        }

        foreach (var (map, chunks) in chunksToLoad)
        {
            foreach (var chunk in chunks)
            {
                var ent = GetOrCreateChunk(chunk, map); // Ensure everything loads.
                if (ent is not null)
                    EnsureComp<LoadedChunkComponent>(ent.Value);
            }
        }
    }

    public EntityUid? GetOrCreateChunk(Vector2i chunk, EntityUid map)
    {
        if (!TryComp<WorldControllerComponent>(map, out var controller))
        {
            throw new Exception($"tried to use {ToPrettyString(map)} as a world map, without actually being one.");
        }

        if (controller.Chunks.TryGetValue(chunk, out var ent))
        {
            return ent;
        }
        else
        {
            return CreateChunkEntity(chunk, map);
        }
    }

    private EntityUid CreateChunkEntity(Vector2i chunkCoords, EntityUid map)
    {
        var coords = new EntityCoordinates(map, WorldGen.ChunkToWorldCoords(chunkCoords));
        var chunk =  Spawn("ORChunk", coords);
        var md = MetaData(chunk);
        md.EntityName = $"S-{chunkCoords.X}/{chunkCoords.Y}";
        return chunk;
    }
}

public readonly record struct WorldChunkAddedEvent(EntityUid Chunk, Vector2i Coords);

public readonly record struct WorldChunkLoadedEvent(EntityUid Chunk, Vector2i Coords);

public readonly record struct WorldChunkUnloadedEvent(EntityUid Chunk, Vector2i Coords);
