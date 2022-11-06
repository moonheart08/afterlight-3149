using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Server._00OuterRim.Worldgen2.Systems;

/// <summary>
/// This handles...
/// </summary>
public abstract class BaseWorldSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;

    [Pure]
    public Vector2i GetChunkCoords(EntityUid ent, TransformComponent? xform = null)
    {
        if (!Resolve(ent, ref xform))
            throw new Exception("Failed to resolve transform, somehow.");

        return WorldGen.WorldToChunkCoords(xform.Coordinates.ToVector2i(EntityManager, _mapManager));
    }

}
