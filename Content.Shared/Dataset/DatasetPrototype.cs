﻿using Robust.Shared.Prototypes;

namespace Content.Shared.Dataset
{
    [Prototype("dataset")]
    public sealed class DatasetPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataFieldAttribute]
        public string ID { get; } = default!;

        [DataField("values")] public IReadOnlyList<string> Values { get; } = new List<string>();
    }
}
