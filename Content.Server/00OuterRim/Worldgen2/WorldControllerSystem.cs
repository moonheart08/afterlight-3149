using Content.Server._00OuterRim.Worldgen2.Components;
using Robust.Shared.Map;

namespace Content.Server._00OuterRim.Worldgen2;

/// <summary>
/// This handles putting together chunk entities and notifying them about important changes.
/// </summary>
public sealed class WorldControllerSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;

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

    public override void Update(float frameTime)
    {

    }
}


public readonly record struct WorldChunkAddedEvent(EntityUid Chunk, Vector2i Coords);
