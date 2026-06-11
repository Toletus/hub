namespace Toletus.Hub.Manager.UI.Models;

public sealed record ConfigurationFieldCapability(
    string Id,
    string LabelKey,
    string GroupKey,
    string ControlKind,
    string? PlaceholderKey = null,
    bool RequiresConnection = true);
