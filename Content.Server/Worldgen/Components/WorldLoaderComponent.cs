using Robust.Shared.GameObjects;

namespace Content.Server.Worldgen.Components;

/// <summary>
/// Objects that have special permission to load in and out the game world.
/// </summary>
public class WorldLoaderComponent : Component
{
    public override string Name => "WorldLoader";
}
