namespace Toletus.Hub.Manager.UI.Models;

public sealed record DeviceDisplayStatusViewModel
{
    public required DeviceDisplayStatusKind Status { get; init; }
    public required string LabelKey { get; init; }
    public required string CssClass { get; init; }
}
