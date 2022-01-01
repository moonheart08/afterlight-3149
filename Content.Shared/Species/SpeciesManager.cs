using System.Collections.Generic;
using System.Linq;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Shared.Species;

public class SpeciesManager
{
    public const string DefaultSpecies = "Human";

    public IReadOnlyDictionary<string, string> SpeciesNameToId = IoCManager.Resolve<IPrototypeManager>()
        .EnumeratePrototypes<SpeciesPrototype>().ToDictionary(x => x.Name, x => x.ID);

}
