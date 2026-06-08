using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class ManagerUiState(IHubDeviceManager deviceManager, NotificationHistoryService history)
{
    private readonly List<DeviceRefViewModel> _devices = [];

    public event Action? Changed;

    public IReadOnlyList<string> Networks { get; private set; } = Array.Empty<string>();
    public string? SelectedNetwork { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public bool IsBusy { get; private set; }
    public string? CurrentCommandId { get; private set; }
    public DeviceRefViewModel? SelectedDevice { get; private set; }
    public Dictionary<string, string> ConfigurationValues { get; } = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<DeviceRefViewModel> Devices => _devices;

    public IEnumerable<DeviceRefViewModel> FilteredDevices =>
        string.IsNullOrWhiteSpace(SearchText)
            ? _devices
            : _devices.Where(d =>
                d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                d.IpAddress.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                d.TypeLabel.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (d.SerialNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Networks = await deviceManager.GetNetworksAsync(cancellationToken);
        var defaultNetwork = await deviceManager.GetDefaultNetworkNameAsync(cancellationToken);
        SelectedNetwork = Networks.Contains(defaultNetwork) ? defaultNetwork : null;
        Changed?.Invoke();
    }

    public async Task DiscoverAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(SelectedNetwork))
        {
            history.Add("discovery", CommandResultViewModel.Warning("Message.NetworkMissing"));
            Changed?.Invoke();
            return;
        }

        await RunBusyAsync(async () =>
        {
            try
            {
                history.Add("discovery", CommandResultViewModel.Pending("Message.DiscoveryPending"));
                _devices.Clear();
                _devices.AddRange(await deviceManager.DiscoverDevicesAsync(SelectedNetwork, cancellationToken));
                SelectedDevice = _devices.FirstOrDefault();
                history.Add("discovery", CommandResultViewModel.Success("Message.DiscoverySuccess", SelectedDevice));
            }
            catch (Exception ex)
            {
                history.Add("discovery", CommandResultViewModel.Error("Message.CommandFailed", ex.Message));
            }
        }, "discovery");
    }

    public async Task SelectDeviceAsync(DeviceRefViewModel device, CancellationToken cancellationToken = default)
    {
        SelectedDevice = device;
        if (device.Connected)
            await LoadConfigurationAsync(cancellationToken);
        else
            ConfigurationValues.Clear();

        Changed?.Invoke();
    }

    public async Task ExecuteAsync(string commandId, CancellationToken cancellationToken = default)
    {
        if (SelectedDevice is null)
            return;

        await RunBusyAsync(async () =>
        {
            history.Add(commandId, CommandResultViewModel.Pending("Message.CommandPending", SelectedDevice));
            var result = commandId switch
            {
                "connection.connect" => await deviceManager.ConnectAsync(SelectedDevice, SelectedNetwork, cancellationToken),
                "connection.disconnect" => await deviceManager.DisconnectAsync(SelectedDevice, cancellationToken),
                _ => await deviceManager.ExecuteCommandAsync(new DeviceCommandRequestViewModel
                {
                    Device = SelectedDevice,
                    CommandId = commandId,
                    Message = "Toletus Hub Manager"
                }, cancellationToken)
            };

            if (result.Device is not null)
            {
                ReplaceDevice(result.Device);
                if (result.IsSuccess && result.Device.Connected)
                    await LoadConfigurationAsync(cancellationToken);
            }

            history.Add(commandId, result);
        }, commandId);
    }

    public async Task ConnectSerialAsync(CancellationToken cancellationToken = default)
    {
        await RunBusyAsync(async () =>
        {
            const string commandId = "connection.connect_serial";
            history.Add(commandId, CommandResultViewModel.Pending("Message.CommandPending"));
            var result = await deviceManager.ConnectSerialAsync(null, cancellationToken);

            if (result.Device is not null)
            {
                ReplaceDevice(result.Device);
                SelectedDevice = result.Device;
                if (result.IsSuccess)
                    await LoadConfigurationAsync(cancellationToken);
            }

            history.Add(commandId, result);
        }, "connection.connect_serial");
    }

    public async Task SendConfigurationAsync(string moduleKey, CancellationToken cancellationToken = default)
    {
        if (SelectedDevice is null)
            return;

        await RunBusyAsync(async () =>
        {
            var commandId = $"configuration.submit.{moduleKey}";
            history.Add(commandId, CommandResultViewModel.Pending("Message.CommandPending", SelectedDevice));
            var result = await deviceManager.SendConfigurationAsync(new DeviceConfigurationViewModel
            {
                Device = SelectedDevice,
                Values = new Dictionary<string, string>(ConfigurationValues, StringComparer.OrdinalIgnoreCase)
            }, moduleKey, cancellationToken);
            history.Add(commandId, result);
        }, $"configuration.submit.{moduleKey}");
    }

    public async Task RefreshConfigurationAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedDevice is null || !SelectedDevice.Connected)
            return;

        await RunBusyAsync(async () =>
        {
            const string commandId = "configuration.refresh";
            history.Add(commandId, CommandResultViewModel.Pending("Message.CommandPending", SelectedDevice));

            try
            {
                await LoadConfigurationAsync(cancellationToken);
                history.Add(commandId, CommandResultViewModel.Success("Message.ConfigurationLoaded", SelectedDevice));
            }
            catch (Exception ex)
            {
                history.Add(commandId, CommandResultViewModel.Error("Message.CommandFailed", ex.Message, SelectedDevice));
            }
        }, "configuration.refresh");
    }

    private async Task LoadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedDevice is null)
            return;

        var configuration = await deviceManager.LoadConfigurationAsync(SelectedDevice, cancellationToken);
        ConfigurationValues.Clear();
        foreach (var item in configuration.Values)
            ConfigurationValues[item.Key] = item.Value;
    }

    private void ReplaceDevice(DeviceRefViewModel device)
    {
        var index = _devices.FindIndex(d => d.Key == device.Key);
        if (index >= 0)
            _devices[index] = device;

        if (SelectedDevice?.Key == device.Key)
            SelectedDevice = device;
    }

    private async Task RunBusyAsync(Func<Task> action, string? commandId = null)
    {
        IsBusy = true;
        CurrentCommandId = commandId;
        Changed?.Invoke();

        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
            CurrentCommandId = null;
            Changed?.Invoke();
        }
    }
}
