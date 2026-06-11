namespace Toletus.Hub.Manager.UI.Models;

public sealed record DeviceRefViewModel
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string IpAddress { get; init; }
    public required DeviceTypeKind Type { get; init; }
    public int Id { get; init; }
    public string? SerialNumber { get; init; }
    public int Port { get; init; }
    public bool Connected { get; init; }
    public IReadOnlyList<DeviceModuleKind> Modules { get; init; } = Array.Empty<DeviceModuleKind>();

    public string TypeLabel => Type.ToString();
}
