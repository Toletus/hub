namespace Toletus.Hub.Manager.UI.Models;

public sealed record DeviceConfigurationViewModel
{
    public required DeviceRefViewModel Device { get; init; }

    public IReadOnlyDictionary<string, string> Values { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
