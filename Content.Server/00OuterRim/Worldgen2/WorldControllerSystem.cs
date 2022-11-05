using Content.Server._00OuterRim.Worldgen2.Components;
using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._00OuterRim.Worldgen2;

/// <summary>
/// This handles putting together chunk entities and notifying them about important changes.
/// </summary>
public sealed class WorldControllerSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WorldChunkComponent, ComponentStartup>(OnChunkStartup);
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

    public override void Update(float frameTime)
    {
        //TODO: Use struct enumerator for this once available.
        //TODO: Maybe don't allocate a big collection every frame?
        var chunksToLoad = new Dictionary<EntityUid, HashSet<Vector2i>>();

        foreach (var controller in EntityQuery<WorldControllerComponent>())
        {
            chunksToLoad[controller.Owner] = new();
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


    }

    private EntityUid CreateChunkEntity(Vector2i chunkCoords, EntityUid map)
    {
        var coords = new EntityCoordinates(map, WorldGen.ChunkToWorldCoords(chunkCoords));
        return Spawn("ORChunk", coords);
    }
}

public readonly record struct WorldChunkAddedEvent(EntityUid Chunk, Vector2i Coords);

public readonly record struct WorldChunkLoadedEvent(EntityUid Chunk, Vector2i Coords);
