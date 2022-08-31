﻿using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Client._00OuterRim;

[Serializable, DataDefinition, Virtual]
public class GuideEntry
{
    [DataField("text", required: true)] public ResourcePath Text = default!;

    public virtual string Id { get; } = default!;

    [DataField("name", required: true)] public string Name = default!;

    [DataField("parent")] public string? Parent = default!;
}
