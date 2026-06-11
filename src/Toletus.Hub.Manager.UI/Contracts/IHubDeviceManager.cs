using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Contracts;

public interface IHubDeviceManager
{
    Task<IReadOnlyList<string>> GetNetworksAsync(CancellationToken cancellationToken = default);

    Task<string?> GetDefaultNetworkNameAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceRefViewModel>> DiscoverDevicesAsync(
        string? networkName,
        CancellationToken cancellationToken = default);

    Task<CommandResultViewModel> ConnectAsync(
        DeviceRefViewModel device,
        string? networkName,
        CancellationToken cancellationToken = default);

    Task<CommandResultViewModel> DisconnectAsync(
        DeviceRefViewModel device,
        CancellationToken cancellationToken = default);

    Task<CommandResultViewModel> ConnectSerialAsync(
        string? portName = null,
        CancellationToken cancellationToken = default);

    Task<DeviceConfigurationViewModel> LoadConfigurationAsync(
        DeviceRefViewModel device,
        CancellationToken cancellationToken = default);

    Task<CommandResultViewModel> SendConfigurationAsync(
        DeviceConfigurationViewModel configuration,
        string moduleKey,
        CancellationToken cancellationToken = default);

    Task<CommandResultViewModel> ExecuteCommandAsync(
        DeviceCommandRequestViewModel request,
        CancellationToken cancellationToken = default);
}
