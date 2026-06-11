using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class ManagerUiState(
    IHubDeviceManager deviceManager,
    NotificationHistoryService history,
    ILogger<ManagerUiState> logger)
{
    private readonly List<DeviceRefViewModel> _devices = [];
    private int _activeExecuteAsync;
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

        if (IsBusy || Interlocked.CompareExchange(ref _activeExecuteAsync, 1, 0) != 0)
        {
            logger.LogInformation(
                "HubManagerDiag ExecuteAsync duplicate-blocked command={CommandId} activeCommand={ActiveCommandId} device={DeviceKey} thread={ThreadId}",
                commandId,
                CurrentCommandId,
                SelectedDevice.Key,
                Environment.CurrentManagedThreadId);
            history.Add(commandId, CommandResultViewModel.Warning("Message.CommandAlreadyInProgress", CurrentCommandId, SelectedDevice));
            Changed?.Invoke();
            return;
        }

        var totalTimestamp = Stopwatch.GetTimestamp();
        logger.LogInformation(
            "HubManagerDiag ExecuteAsync start command={CommandId} device={DeviceKey} type={DeviceType} connected={Connected} thread={ThreadId}",
            commandId,
            SelectedDevice.Key,
            SelectedDevice.Type,
            SelectedDevice.Connected,
            Environment.CurrentManagedThreadId);

        try
        {
            await RunBusyAsync(async () =>
            {
                var pendingTimestamp = Stopwatch.GetTimestamp();
                history.Add(commandId, CommandResultViewModel.Pending("Message.CommandPending", SelectedDevice));
                logger.LogInformation(
                    "HubManagerDiag ExecuteAsync pending-history-added command={CommandId} elapsedMs={ElapsedMs} thread={ThreadId}",
                    commandId,
                    Stopwatch.GetElapsedTime(pendingTimestamp).TotalMilliseconds,
                    Environment.CurrentManagedThreadId);

                var deviceManagerTimestamp = Stopwatch.GetTimestamp();
                var result = commandId switch
                {
                    "connection.connect" => await deviceManager.ConnectAsync(SelectedDevice, SelectedNetwork, cancellationToken),
                    "connection.disconnect" => await deviceManager.DisconnectAsync(SelectedDevice, cancellationToken),
                    _ => await deviceManager.ExecuteCommandAsync(new DeviceCommandRequestViewModel
                    {
                        Device = SelectedDevice,
                        CommandId = commandId,
                        Message = "LiteNet Manager"
                    }, cancellationToken)
                };
                logger.LogInformation(
                    "HubManagerDiag ExecuteAsync device-manager-returned command={CommandId} status={Status} elapsedMs={ElapsedMs} thread={ThreadId}",
                    commandId,
                    result.Status,
                    Stopwatch.GetElapsedTime(deviceManagerTimestamp).TotalMilliseconds,
                    Environment.CurrentManagedThreadId);

                if (commandId == "connection.disconnect" && result.IsSuccess)
                {
                    result = result with { Device = MarkDeviceDisconnected(result.Device) };
                }
                else if (result.Device is not null)
                {
                    ReplaceDevice(result.Device);
                }

                var finalHistoryTimestamp = Stopwatch.GetTimestamp();
                history.Add(commandId, result);
                logger.LogInformation(
                    "HubManagerDiag ExecuteAsync final-history-added command={CommandId} status={Status} elapsedMs={ElapsedMs} totalElapsedMs={TotalElapsedMs} thread={ThreadId}",
                    commandId,
                    result.Status,
                    Stopwatch.GetElapsedTime(finalHistoryTimestamp).TotalMilliseconds,
                    Stopwatch.GetElapsedTime(totalTimestamp).TotalMilliseconds,
                    Environment.CurrentManagedThreadId);

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
        finally
        {
            Interlocked.Exchange(ref _activeExecuteAsync, 0);
        }

        logger.LogInformation(
            "HubManagerDiag ExecuteAsync end command={CommandId} totalElapsedMs={TotalElapsedMs} thread={ThreadId}",
            commandId,
            Stopwatch.GetElapsedTime(totalTimestamp).TotalMilliseconds,
            Environment.CurrentManagedThreadId);
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
        var timestamp = Stopwatch.GetTimestamp();
        IsBusy = true;
        CurrentCommandId = commandId;
        Changed?.Invoke();
        logger.LogInformation(
            "HubManagerDiag RunBusyAsync busy-set command={CommandId} elapsedMs={ElapsedMs} thread={ThreadId}",
            commandId,
            Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds,
            Environment.CurrentManagedThreadId);

        try
        {
            await action();
        }
        finally
        {
            var clearTimestamp = Stopwatch.GetTimestamp();
            IsBusy = false;
            CurrentCommandId = null;
            Changed?.Invoke();
            logger.LogInformation(
                "HubManagerDiag RunBusyAsync busy-cleared command={CommandId} clearElapsedMs={ClearElapsedMs} totalElapsedMs={TotalElapsedMs} thread={ThreadId}",
                commandId,
                Stopwatch.GetElapsedTime(clearTimestamp).TotalMilliseconds,
                Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds,
                Environment.CurrentManagedThreadId);
        }
    }
}
