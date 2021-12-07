using Robust.Shared.GameObjects;

namespace Content.Server.Worldgen.EntitySystems;

/// <summary>
///     Manages loading and unloading objects in sectors, with a mix of a chunk system and a "point of interest" system.
/// </summary>
/// <remarks>
///     Large structures that do not fit in a 16x16 area should likely use the point of interest system.
///     Other objects, like small asteroids, should be dynamic, and use the chunk system (and also NOT SAVE ANY DATA)
///
/// </remarks>
public class DynamicLoadSystem : EntitySystem
{

    public override void Initialize()
    {

    }

    public override void Update(float frameTime)
    {

    }
}
