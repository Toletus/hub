namespace Toletus.Hub.Manager.UI.Models;

public sealed record CommandHistoryItemViewModel
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string CommandId { get; init; }
    public required CommandStatusKind Status { get; init; }
    public required string MessageKey { get; init; }
    public string? DeviceName { get; init; }
    public string? TechnicalDetails { get; init; }
}
