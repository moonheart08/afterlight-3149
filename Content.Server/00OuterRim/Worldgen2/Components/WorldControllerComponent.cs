namespace Content.Server._00OuterRim.Worldgen2.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, Access(typeof(WorldControllerSystem))]
public sealed class WorldControllerComponent : Component
{
    public Dictionary<Vector2i, EntityUid> Chunks = new();
}
