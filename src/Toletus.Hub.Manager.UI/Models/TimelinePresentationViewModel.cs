namespace Toletus.Hub.Manager.UI.Models;

public sealed record TimelinePresentationViewModel
{
    public required TimelineEventKind Kind { get; init; }
    public required TimelinePresentationSeverity Severity { get; init; }
    public required string KindCssClass { get; init; }
    public required string KindLabelKey { get; init; }
    public required string SeverityCssClass { get; init; }
}
