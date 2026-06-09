using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class ManagerUiState(IHubDeviceManager deviceManager, NotificationHistoryService history)
{
    private readonly List<DeviceRefViewModel> _devices = [];
    private int _configurationLoadVersion;

    public event Action? Changed;

    public IReadOnlyList<string> Networks { get; private set; } = Array.Empty<string>();
    public string? SelectedNetwork { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public bool IsBusy { get; private set; }
    public bool IsConfigurationLoading { get; private set; }
    public string? CurrentCommandId { get; private set; }
    public string? CurrentConfigurationCommandId { get; private set; }
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
        Changed?.Invoke();

        if (device.Connected)
            StartConfigurationLoad(cancellationToken);
        else
        {
            _configurationLoadVersion++;
            IsConfigurationLoading = false;
            CurrentConfigurationCommandId = null;
            ConfigurationValues.Clear();
            Changed?.Invoke();
        }

        await Task.CompletedTask;
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

            if (commandId == "connection.disconnect" && result.IsSuccess)
            {
                result = result with { Device = MarkDeviceDisconnected(result.Device) };
            }
            else if (result.Device is not null)
            {
                ReplaceDevice(result.Device);
            }

            history.Add(commandId, result);

            if (commandId == "connection.connect" && result.IsSuccess && result.Device?.Connected == true)
                StartConfigurationLoad(cancellationToken);
            else if (commandId == "connection.disconnect" && result.IsSuccess)
            {
                _configurationLoadVersion++;
                IsConfigurationLoading = false;
                CurrentConfigurationCommandId = null;
                ConfigurationValues.Clear();
                Changed?.Invoke();
            }
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
            }

            history.Add(commandId, result);

            if (result.IsSuccess && result.Device?.Connected == true)
                StartConfigurationLoad(cancellationToken);
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

        const string commandId = "configuration.refresh";
        history.Add(commandId, CommandResultViewModel.Pending("Message.CommandPending", SelectedDevice));
        await LoadConfigurationAsync(commandId, addHistory: true, cancellationToken);
    }

    private void StartConfigurationLoad(CancellationToken cancellationToken = default)
    {
        _ = LoadConfigurationAsync(null, addHistory: false, cancellationToken);
    }

    private async Task LoadConfigurationAsync(
        string? commandId = null,
        bool addHistory = false,
        CancellationToken cancellationToken = default)
    {
        if (SelectedDevice is null)
            return;

        var device = SelectedDevice;
        var loadVersion = ++_configurationLoadVersion;
        IsConfigurationLoading = true;
        CurrentConfigurationCommandId = commandId ?? "configuration.refresh";
        Changed?.Invoke();

        try
        {
            await Task.Yield();
            var configuration = await deviceManager.LoadConfigurationAsync(device, cancellationToken);

            if (SelectedDevice?.Key != device.Key || loadVersion != _configurationLoadVersion)
                return;

            ConfigurationValues.Clear();
            foreach (var item in configuration.Values)
                ConfigurationValues[item.Key] = item.Value;

            if (addHistory)
                history.Add(commandId!, CommandResultViewModel.Success("Message.ConfigurationLoaded", SelectedDevice));
        }
        catch (Exception ex)
        {
            if (addHistory)
                history.Add(commandId!, CommandResultViewModel.Error("Message.CommandFailed", ex.Message, device));
        }
        finally
        {
            if (SelectedDevice?.Key == device.Key && loadVersion == _configurationLoadVersion)
            {
                IsConfigurationLoading = false;
                CurrentConfigurationCommandId = null;
                Changed?.Invoke();
            }
        }
    }

    private void ReplaceDevice(DeviceRefViewModel device)
    {
        var index = _devices.FindIndex(d => d.Key == device.Key);
        if (index >= 0)
            _devices[index] = device;

        if (SelectedDevice?.Key == device.Key)
            SelectedDevice = device;
    }

    private DeviceRefViewModel? MarkDeviceDisconnected(DeviceRefViewModel? device)
    {
        var source = device ?? SelectedDevice;
        if (source is null)
            return null;

        var disconnected = source with
        {
            Connected = false,
            Modules = Array.Empty<DeviceModuleKind>()
        };

        ReplaceDevice(disconnected);
        return disconnected;
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
