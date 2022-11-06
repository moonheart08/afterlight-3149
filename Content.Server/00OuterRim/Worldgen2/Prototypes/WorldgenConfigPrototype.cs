using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._00OuterRim.Worldgen2.Prototypes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("worldgenConfig")]
public sealed class WorldgenConfigPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The components that get added to the target map.
    /// </summary>
    [DataField("components")]
    public EntityPrototype.ComponentRegistry Components { get; } = default!;

    /// <summary>
    /// Applies the worldgen config to the given target (presumably a map.)
    /// </summary>
    public void Apply(EntityUid target, ISerializationManager serialization, IEntityManager entityManager)
    {
        // Add all components required by the prototype. Engine update for this whenst.
        foreach (var entry in Components.Values)
        {
            var comp = (Component) serialization.Copy(entry.Component);
            comp.Owner = target;
            entityManager.AddComponent(target, comp);
        }
    }
}
