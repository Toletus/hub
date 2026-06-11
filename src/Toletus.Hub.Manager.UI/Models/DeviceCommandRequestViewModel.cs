namespace Toletus.Hub.Manager.UI.Models;

public sealed record DeviceCommandRequestViewModel
{
    public required DeviceRefViewModel Device { get; init; }
    public required string CommandId { get; init; }
    public string? Message { get; init; }
    public IReadOnlyDictionary<string, string> Arguments { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
