namespace Content.Server._00OuterRim.Mapping;

/// <summary>
/// This is used for automatic linkage with buttons and other transmitters.
/// </summary>
[RegisterComponent]
public sealed class AutoLinkReceiverComponent : Component
{
    [DataField("channel", required: true, readOnly: true)]
    public string AutoLinkChannel = default!;
}
