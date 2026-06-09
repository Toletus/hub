using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Toletus.Hub.Manager.UI.Capabilities;
using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Models;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services;
using Toletus.LiteNet2.Command.Enums;
using HubDevice = Toletus.Hub.Models.Device;
using HubDeviceType = Toletus.Hub.Models.DeviceType;
using LiteNet1FlowMode = Toletus.LiteNet1.Enums.ModoFluxo;

namespace Toletus.Hub.Manager.Maui.Services;

public sealed class HubDirectDeviceManager(
    DeviceService deviceService,
    ControllerService controllerService,
    BasicCommonCommandService commonCommandService,
    LiteNet1CommandService liteNet1CommandService,
    LiteNet2CommandService liteNet2CommandService,
    LiteNet3CommandService liteNet3CommandService,
    SM25ReaderCommandsService sm25ReaderCommandsService,
    ILogger<HubDirectDeviceManager> logger) : IHubDeviceManager
{
    private readonly Dictionary<string, HubDevice> _devices = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<string>> GetNetworksAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(deviceService.GetNetworksNames().ToList());
    }

    public Task<string?> GetDefaultNetworkNameAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(deviceService.GetDefaultNetworkName());
    }

    public async Task<IReadOnlyList<DeviceRefViewModel>> DiscoverDevicesAsync(string? networkName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await Task.Run(() => deviceService.DiscoverDevices(networkName), cancellationToken);
        if (!response.Success)
            throw new InvalidOperationException(response.Message);

        var devices = HubDevice.AsDevices(response.Data) ?? [];
        return devices.Select(Map).ToList();
    }

    public async Task<CommandResultViewModel> ConnectAsync(
        DeviceRefViewModel device,
        string? networkName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var response = await Task.Run(
                () => controllerService.Connect(device.IpAddress, ToHubType(device.Type), networkName),
                cancellationToken);
            return ToResult(response, "Message.Connected", device);
        }
        catch (Exception ex)
        {
            return CommandResultViewModel.Error("Message.CommandFailed", ex.Message, device);
        }
    }

    public async Task<CommandResultViewModel> DisconnectAsync(DeviceRefViewModel device, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var response = await Task.Run(
                () => controllerService.Disconnect(device.IpAddress, ToHubType(device.Type)),
                cancellationToken);
            return ToResult(response, "Message.Disconnected", device);
        }
        catch (Exception ex)
        {
            return CommandResultViewModel.Error("Message.CommandFailed", ex.Message, device);
        }
    }

    public async Task<CommandResultViewModel> ConnectSerialAsync(string? portName = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var response = await Task.Run(() => controllerService.ConnectSerialPort(portName), cancellationToken);
            var device = HubDevice.AsDevice(response.Data);
            var mapped = device is null ? null : Map(device);

            return response.Success && mapped is not null
                ? CommandResultViewModel.Success("Message.Connected", mapped, response.Data)
                : CommandResultViewModel.Error("Message.CommandFailed", response.Message, mapped);
        }
        catch (Exception ex)
        {
            return CommandResultViewModel.Error("Message.CommandFailed", ex.Message);
        }
    }

    public async Task<CommandResultViewModel> ExecuteCommandAsync(
        DeviceCommandRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        var totalTimestamp = Stopwatch.GetTimestamp();
        logger.LogInformation(
            "HubManagerDiag HubDirect.ExecuteCommand start command={CommandId} device={DeviceKey} type={DeviceType} thread={ThreadId}",
            request.CommandId,
            request.Device.Key,
            request.Device.Type,
            Environment.CurrentManagedThreadId);

        cancellationToken.ThrowIfCancellationRequested();

        if (!_devices.TryGetValue(request.Device.Key, out var device))
        {
            logger.LogInformation(
                "HubManagerDiag HubDirect.ExecuteCommand cache-miss command={CommandId} device={DeviceKey} elapsedMs={ElapsedMs} thread={ThreadId}",
                request.CommandId,
                request.Device.Key,
                Stopwatch.GetElapsedTime(totalTimestamp).TotalMilliseconds,
                Environment.CurrentManagedThreadId);
            return CommandResultViewModel.Error("Message.CommandFailed", "Device is not present in the adapter cache.", request.Device);
        }

        try
        {
            var result = request.CommandId switch
            {
                DeviceCapabilityCatalog.ReleaseEntry => ToResult(await ExecuteCommonReleaseAsync(
                        request.CommandId,
                        request.Device,
                        () => commonCommandService.ReleaseEntry(device, request.Message ?? string.Empty),
                        cancellationToken),
                    "Message.CommandSuccess",
                    request.Device),
                DeviceCapabilityCatalog.ReleaseEntryAndExit => ToResult(await ExecuteCommonReleaseAsync(
                        request.CommandId,
                        request.Device,
                        () => commonCommandService.ReleaseEntryAndExit(device, request.Message ?? string.Empty),
                        cancellationToken),
                    "Message.CommandSuccess",
                    request.Device),
                DeviceCapabilityCatalog.ReleaseExit => ToResult(await ExecuteCommonReleaseAsync(
                        request.CommandId,
                        request.Device,
                        () => commonCommandService.ReleaseExit(device, request.Message ?? string.Empty),
                        cancellationToken),
                    "Message.CommandSuccess",
                    request.Device),
                DeviceCapabilityCatalog.Reset => await ExecuteResetAsync(device, request.Device),
                DeviceCapabilityCatalog.ResetCounters => await ExecuteResetCountersAsync(device, request.Device),
                DeviceCapabilityCatalog.GetStatus => await ExecuteStatusAsync(device, request.Device),
                DeviceCapabilityCatalog.Sm25ReaderInfo => await ToResultAsync(sm25ReaderCommandsService.GetDeviceName(device), request.Device),
                DeviceCapabilityCatalog.Sm25Cancel => await ToResultAsync(sm25ReaderCommandsService.FPCancel(device), request.Device),
                _ => CommandResultViewModel.Warning("Message.CommandUnsupported", request.CommandId, request.Device)
            };

            logger.LogInformation(
                "HubManagerDiag HubDirect.ExecuteCommand end command={CommandId} status={Status} elapsedMs={ElapsedMs} thread={ThreadId}",
                request.CommandId,
                result.Status,
                Stopwatch.GetElapsedTime(totalTimestamp).TotalMilliseconds,
                Environment.CurrentManagedThreadId);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogInformation(
                ex,
                "HubManagerDiag HubDirect.ExecuteCommand exception command={CommandId} elapsedMs={ElapsedMs} thread={ThreadId}",
                request.CommandId,
                Stopwatch.GetElapsedTime(totalTimestamp).TotalMilliseconds,
                Environment.CurrentManagedThreadId);
            return CommandResultViewModel.Error("Message.CommandFailed", ex.Message, request.Device);
        }
    }

    private async Task<DeviceResponse> ExecuteCommonReleaseAsync(
        string commandId,
        DeviceRefViewModel device,
        Func<DeviceResponse> action,
        CancellationToken cancellationToken)
    {
        var totalTimestamp = Stopwatch.GetTimestamp();
        logger.LogInformation(
            "HubManagerDiag HubDirect.CommonRelease schedule command={CommandId} device={DeviceKey} thread={ThreadId}",
            commandId,
            device.Key,
            Environment.CurrentManagedThreadId);

        var response = await Task.Run(() =>
        {
            var workerTimestamp = Stopwatch.GetTimestamp();
            logger.LogInformation(
                "HubManagerDiag HubDirect.CommonRelease worker-start command={CommandId} device={DeviceKey} thread={ThreadId}",
                commandId,
                device.Key,
                Environment.CurrentManagedThreadId);

            try
            {
                return action();
            }
            finally
            {
                logger.LogInformation(
                    "HubManagerDiag HubDirect.CommonRelease worker-end command={CommandId} device={DeviceKey} elapsedMs={ElapsedMs} thread={ThreadId}",
                    commandId,
                    device.Key,
                    Stopwatch.GetElapsedTime(workerTimestamp).TotalMilliseconds,
                    Environment.CurrentManagedThreadId);
            }
        }, cancellationToken);

        logger.LogInformation(
            "HubManagerDiag HubDirect.CommonRelease returned command={CommandId} device={DeviceKey} success={Success} elapsedMs={ElapsedMs} thread={ThreadId}",
            commandId,
            device.Key,
            response.Success,
            Stopwatch.GetElapsedTime(totalTimestamp).TotalMilliseconds,
            Environment.CurrentManagedThreadId);

        return response;
    }

    public async Task<DeviceConfigurationViewModel> LoadConfigurationAsync(
        DeviceRefViewModel device,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["device_id"] = device.Id.ToString(),
            ["static_ip"] = device.IpAddress,
            ["firmware_version"] = "-",
            ["entered"] = "0",
            ["exited"] = "0"
        };

        if (!_devices.TryGetValue(device.Key, out var hubDevice))
            return new DeviceConfigurationViewModel { Device = device, Values = values };

        if (hubDevice.Type == HubDeviceType.LiteNet3)
        {
            var statusAndConfigurations = liteNet3CommandService.GetStatusAndConfigurations(hubDevice);
            await TryPopulateAsync(values, "device_id", statusAndConfigurations);
            await TryPopulateAsync(values, "firmware_version", statusAndConfigurations);
            await TryPopulateAsync(values, "release_duration", statusAndConfigurations);
            await TryPopulateAsync(values, "menu_password", statusAndConfigurations);

            var ethernet = liteNet3CommandService.GetEthernet(hubDevice);
            await TryPopulateAsync(values, "mac", ethernet);
            await TryPopulateAsync(values, "static_ip", ethernet);
            await TryPopulateAsync(values, "subnet_mask", ethernet);
            await TryPopulateAsync(values, "gateway", ethernet);
            await TryPopulateAsync(values, "ip_mode", ethernet);

            var flow = liteNet3CommandService.GetFlow(hubDevice);
            await TryPopulateAsync(values, "flow_mode", flow);
            await TryPopulateAsync(values, "l3_flow_inverted", flow);
            await TryPopulateAsync(values, "l3_flow_out", flow);
            await TryPopulateAsync(values, "l3_flow_front_wait", flow);
            await TryPopulateAsync(values, "l3_flow_picto_wait_in", flow);
            await TryPopulateAsync(values, "l3_flow_picto_wait_out", flow);

            var display = liteNet3CommandService.GetDisplay(hubDevice);
            await TryPopulateAsync(values, "default_message", display);
            await TryPopulateAsync(values, "secondary_message", display);

            return new DeviceConfigurationViewModel { Device = device, Values = values };
        }

        await TryPopulateAsync(values, "device_id", hubDevice.Type switch
        {
            HubDeviceType.LiteNet1 => liteNet1CommandService.GetId(hubDevice),
            HubDeviceType.LiteNet2 => liteNet2CommandService.GetId(hubDevice),
            _ => Task.FromResult(new Notification(hubDevice.Ip, hubDevice.Id, 0, hubDevice.Type))
        });

        await TryPopulateAsync(values, "firmware_version", hubDevice.Type switch
        {
            HubDeviceType.LiteNet1 => liteNet1CommandService.GetFirmwareVersion(hubDevice),
            HubDeviceType.LiteNet2 => liteNet2CommandService.GetFirmwareVersion(hubDevice),
            _ => Task.FromResult(new Notification(hubDevice.Ip, hubDevice.Id, 0, hubDevice.Type))
        });

        if (hubDevice.Type == HubDeviceType.LiteNet2)
        {
            await TryPopulateAsync(values, "mac", liteNet2CommandService.GetMac(hubDevice));
            await TryPopulateAsync(values, "entered", liteNet2CommandService.GetCounters(hubDevice));
            await TryPopulateAsync(values, "exited", liteNet2CommandService.GetCounters(hubDevice));
            await TryPopulateAsync(values, "menu_password", liteNet2CommandService.GetMenuPassword(hubDevice));
            await TryPopulateAsync(values, "default_message", liteNet2CommandService.GetMessageLine1(hubDevice));
            await TryPopulateAsync(values, "secondary_message", liteNet2CommandService.GetMessageLine2(hubDevice));
            await TryPopulateAsync(values, "release_duration", liteNet2CommandService.GetReleaseDuration(hubDevice));
            await TryPopulateAsync(values, "show_counters", liteNet2CommandService.GetShowCounters(hubDevice));
            await TryPopulateAsync(values, "flow_mode", liteNet2CommandService.GetFlowControl(hubDevice));
            await TryPopulateAsync(values, "ip_mode", liteNet2CommandService.GetIpMode(hubDevice));
        }

        if (hubDevice.Type == HubDeviceType.LiteNet1)
        {
            await TryPopulateAsync(values, "entered", liteNet1CommandService.GetCounters(hubDevice));
            await TryPopulateAsync(values, "exited", liteNet1CommandService.GetCounters(hubDevice));
            await TryPopulateAsync(values, "ip_mode", liteNet1CommandService.GetIpMode(hubDevice));
            await TryPopulateAsync(values, "show_counters", liteNet1CommandService.GetShowCounters(hubDevice));
        }

        return new DeviceConfigurationViewModel { Device = device, Values = values };
    }

    public async Task<CommandResultViewModel> SendConfigurationAsync(
        DeviceConfigurationViewModel configuration,
        string moduleKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_devices.TryGetValue(configuration.Device.Key, out var device))
            return CommandResultViewModel.Error("Message.CommandFailed", "Device is not present in the adapter cache.", configuration.Device);

        try
        {
            var results = await SendModuleConfigurationAsync(device, configuration, moduleKey, cancellationToken);
            if (results.Count == 0)
            {
                return CommandResultViewModel.Warning(
                    "Message.CommandUnsupported",
                    moduleKey,
                    configuration.Device);
            }

            var failed = results.FirstOrDefault(result => !result.IsSuccess);
            return failed ?? CommandResultViewModel.Success(
                "Message.ConfigurationSent",
                configuration.Device,
                moduleKey);
        }
        catch (Exception ex)
        {
            return CommandResultViewModel.Error("Message.CommandFailed", ex.Message, configuration.Device);
        }
    }

    private DeviceRefViewModel Map(HubDevice device)
    {
        var mappedType = ToUiType(device.Type);
        var key = $"{mappedType}:{device.Ip}:{device.Id}";
        IReadOnlyList<DeviceModuleKind> modules = mappedType == DeviceTypeKind.LiteNet2 && HasFingerprintReader(device)
            ? [DeviceModuleKind.SM25]
            : Array.Empty<DeviceModuleKind>();

        var viewModel = new DeviceRefViewModel
        {
            Key = key,
            Name = device.Name,
            IpAddress = device.Ip,
            SerialNumber = device.SerialNumber,
            Port = device.Port,
            Connected = device.Connected,
            Type = mappedType,
            Id = device.Id,
            Modules = modules
        };

        _devices[key] = device;
        return viewModel;
    }

    private static bool HasFingerprintReader(HubDevice device)
    {
        if (device.Type != HubDeviceType.LiteNet2 || !device.Connected)
            return false;

        var board = device.Get();
        if (board is null)
            return false;

        foreach (var propertyName in new[]
                 {
                     "FingerprintReaderConnected",
                     "IsFingerprintReaderConnected",
                     "HasFingerprintReader",
                     "FingerprintConnected"
                 })
        {
            if (ReadProperty(board, propertyName) is bool connected)
                return connected;
        }

        return false;
    }

    private CommandResultViewModel ToResult(DeviceResponse response, string successKey, DeviceRefViewModel device)
    {
        var mappedDevice = HubDevice.AsDevice(response.Data) is { } hubDevice ? Map(hubDevice) : device;

        return response.Success
            ? CommandResultViewModel.Success(successKey, mappedDevice, response.Data)
            : CommandResultViewModel.Error("Message.CommandFailed", response.Message, mappedDevice);
    }

    private async Task<CommandResultViewModel> ExecuteResetAsync(HubDevice device, DeviceRefViewModel viewModel)
    {
        return device.Type switch
        {
            HubDeviceType.LiteNet1 => await ToResultAsync(liteNet1CommandService.Reset(device), viewModel),
            HubDeviceType.LiteNet2 => await ToResultAsync(liteNet2CommandService.Reset(device), viewModel),
            HubDeviceType.LiteNet3 => await ToResultAsync(liteNet3CommandService.Reset(device), viewModel),
            _ => CommandResultViewModel.Warning("Message.CommandUnsupported", device.Type.ToString(), viewModel)
        };
    }

    private async Task<CommandResultViewModel> ExecuteResetCountersAsync(HubDevice device, DeviceRefViewModel viewModel)
    {
        return device.Type switch
        {
            HubDeviceType.LiteNet1 => await ToResultAsync(liteNet1CommandService.ResetCounters(device), viewModel),
            HubDeviceType.LiteNet2 => await ToResultAsync(liteNet2CommandService.ResetCounters(device), viewModel),
            _ => CommandResultViewModel.Warning("Message.CommandUnsupported", device.Type.ToString(), viewModel)
        };
    }

    private async Task<CommandResultViewModel> ExecuteStatusAsync(HubDevice device, DeviceRefViewModel viewModel)
    {
        return device.Type switch
        {
            HubDeviceType.LiteNet1 => await ToResultAsync(liteNet1CommandService.GetAll(device), viewModel),
            HubDeviceType.LiteNet2 => await ToResultAsync(liteNet2CommandService.GetSerialNumber(device), viewModel),
            HubDeviceType.LiteNet3 => await ToResultAsync(liteNet3CommandService.GetStatusAndConfigurations(device), viewModel),
            _ => CommandResultViewModel.Warning("Message.CommandUnsupported", device.Type.ToString(), viewModel)
        };
    }

    private async Task<List<CommandResultViewModel>> SendModuleConfigurationAsync(
        HubDevice device,
        DeviceConfigurationViewModel configuration,
        string moduleKey,
        CancellationToken cancellationToken)
    {
        var results = new List<CommandResultViewModel>();
        var values = configuration.Values;

        async Task Add(Task<Notification> command)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await ToResultAsync(command, configuration.Device));
        }

        switch (moduleKey)
        {
            case "Config.General":
                if (TryGetInt(values, "device_id", out var id))
                {
                    device.Id = id;
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet1 => liteNet1CommandService.SetId(device, id),
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetId(device),
                        HubDeviceType.LiteNet3 => liteNet3CommandService.SetId(device, id),
                        _ => UnsupportedNotification(device)
                    });
                }
                break;

            case "Config.Flow":
                if (TryGet(values, "flow_mode", out var flowMode))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet1 => liteNet1CommandService.SetFlowControl(device, ToLiteNet1Flow(flowMode)),
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetFlowControl(device, ToLiteNet2Flow(flowMode)),
                        HubDeviceType.LiteNet3 => liteNet3CommandService.SetFlow(
                            device,
                            TryGetBool(values, "l3_flow_inverted", out var inverted) && inverted,
                            ToLiteNet3FlowDirection(flowMode),
                            GetValue(values, "l3_flow_out") ?? "Out",
                            GetValue(values, "l3_flow_front_wait") ?? "None",
                            TryGetInt(values, "l3_flow_picto_wait_in", out var pictoWaitIn) ? pictoWaitIn : 0,
                            TryGetInt(values, "l3_flow_picto_wait_out", out var pictoWaitOut) ? pictoWaitOut : 0),
                        _ => UnsupportedNotification(device)
                    });
                }
                break;

            case "Config.Counters":
                if (TryGetBool(values, "show_counters", out var showCounters))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet1 => liteNet1CommandService.SetShowCounters(device, showCounters),
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetShowCounters(device, showCounters),
                        _ => UnsupportedNotification(device)
                    });
                }
                break;

            case "Config.Operation":
                if (TryGetBool(values, "entry_clockwise", out var entryClockwise))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet1 => liteNet1CommandService.SetEntryClockwise(device, entryClockwise),
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetEntryClockwise(device, entryClockwise),
                        _ => UnsupportedNotification(device)
                    });
                }

                if (TryGetBool(values, "buzzer_mute", out var buzzerMute))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet1 => liteNet1CommandService.SetBuzzerMute(device, buzzerMute),
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetBuzzerMute(device, buzzerMute),
                        HubDeviceType.LiteNet3 => liteNet3CommandService.SetBuzzerMute(device, buzzerMute),
                        _ => UnsupportedNotification(device)
                    });
                }

                if (TryGetInt(values, "release_duration", out var releaseDuration))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetReleaseDuration(device, releaseDuration),
                        HubDeviceType.LiteNet3 => liteNet3CommandService.SetReleaseDuration(device, releaseDuration),
                        _ => UnsupportedNotification(device)
                    });
                }
                break;

            case "Config.Messages":
                if (TryGet(values, "default_message", out var defaultMessage))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetMessageLine1(device, defaultMessage),
                        HubDeviceType.LiteNet3 => liteNet3CommandService.SetDisplay(
                            device,
                            defaultMessage,
                            GetValue(values, "secondary_message"),
                            GetValue(values, "display_mode")),
                        _ => UnsupportedNotification(device)
                    });
                }

                if (device.Type == HubDeviceType.LiteNet2 && TryGet(values, "secondary_message", out var secondaryMessage))
                    await Add(liteNet2CommandService.SetMessageLine2(device, secondaryMessage));
                break;

            case "Config.Biometrics":
                if (device.Type == HubDeviceType.LiteNet2 &&
                    TryGet(values, "fingerprint_identification_mode", out var fingerprintMode) &&
                    Enum.TryParse<FingerprintIdentificationMode>(fingerprintMode, ignoreCase: true, out var parsedFingerprintMode))
                {
                    await Add(liteNet2CommandService.SetFingerprintIdentificationMode(device, parsedFingerprintMode));
                }
                break;

            case "Config.Notification":
                if (device.Type == HubDeviceType.LiteNet2)
                {
                    var duration = TryGetInt(values, "release_duration", out var notifyDuration) ? notifyDuration * 1000 : 1000;
                    var tone = TryGetInt(values, "notify_tone", out var notifyTone) ? notifyTone : 0;
                    var color = TryGetInt(values, "notify_color", out var notifyColor) ? notifyColor : 0;
                    var showMessage = TryGetBool(values, "notify_show_message", out var notifyShowMessage) && notifyShowMessage ? 1 : 0;
                    await Add(liteNet2CommandService.Notify(device, duration, tone, color, showMessage));
                }
                break;

            case "Config.Network":
                if (device.Type == HubDeviceType.LiteNet2)
                {
                    if (TryGet(values, "ip_mode", out var ipMode) && TryGet(values, "static_ip", out var ip) && TryGet(values, "subnet_mask", out var mask))
                        await Add(liteNet2CommandService.SetIp(device, IsDhcp(ipMode), ip, mask));

                    if (TryGet(values, "mac", out var mac))
                        await Add(liteNet2CommandService.SetMac(device, mac));
                }
                else if (device.Type == HubDeviceType.LiteNet3)
                {
                    if (TryGet(values, "ip_mode", out var ipMode))
                        await Add(liteNet3CommandService.SetStaticIp(device, !IsDhcp(ipMode)));

                    if (TryGet(values, "static_ip", out var ip) && TryGet(values, "subnet_mask", out var mask))
                        await Add(liteNet3CommandService.SetIpConfigurartion(device, ip, mask, GetValue(values, "gateway") ?? "0.0.0.0"));

                    if (TryGet(values, "mac", out var mac))
                        await Add(liteNet3CommandService.SetMac(device, mac));
                }
                break;

            case "Config.Security":
                if (TryGet(values, "menu_password", out var password))
                {
                    await Add(device.Type switch
                    {
                        HubDeviceType.LiteNet2 => liteNet2CommandService.SetMenuPassword(device, password),
                        HubDeviceType.LiteNet3 => liteNet3CommandService.SetMenuPassword(device, password),
                        _ => UnsupportedNotification(device)
                    });
                }
                break;
        }

        return results;
    }

    private static async Task<CommandResultViewModel> ToResultAsync(Task<Notification> command, DeviceRefViewModel device)
    {
        var notification = await command;
        if (notification.Response is DeviceResponse { Success: false } response)
            return CommandResultViewModel.Error("Message.CommandFailed", response.Message, device) with
            {
                Data = notification.Response
            };

        return CommandResultViewModel.Success("Message.CommandSuccess", device, notification.Response) with
        {
            TechnicalDetails = null
        };
    }

    private static Task<Notification> UnsupportedNotification(HubDevice device) =>
        Task.FromResult(Notification.CreateNotification(
            device.Ip,
            device.Id,
            0,
            device.Type,
            new DeviceResponse(false, "This configuration module is not supported for this device.")));

    private static async Task TryPopulateAsync(
        IDictionary<string, string> values,
        string key,
        Task<Notification> command)
    {
        try
        {
            var notification = await command;
            if (notification.Response is DeviceResponse { Success: false })
                return;

            if (ExtractConfigurationValue(notification.Response, key) is { } value)
                values[key] = value;
        }
        catch
        {
            // Hardware reads are best-effort; the UI keeps editable fallback values.
        }
    }

    private static string? ExtractConfigurationValue(object? response, string key)
    {
        var payload = response is DeviceResponse deviceResponse ? deviceResponse.Data : response;
        payload = ReadProperty(payload, "Content") ?? payload;

        return key switch
        {
            "device_id" => Stringify(ReadProperty(payload, "Id") ?? ReadProperty(payload, "ID") ?? payload),
            "firmware_version" => Stringify(
                ReadProperty(payload, "FirmwareVersion") ??
                ReadProperty(payload, "Firmware") ??
                ReadProperty(payload, "VersaoFirmware") ??
                ScalarOrNull(payload)),
            "mac" => FormatMac(ReadProperty(payload, "Mac") ?? ScalarOrNull(payload)),
            "menu_password" => Stringify(ReadProperty(payload, "MenuPassword") ?? ReadProperty(payload, "MenuPass") ?? ScalarOrNull(payload)),
            "default_message" => Stringify(ReadProperty(payload, "MessageLine1") ?? ReadProperty(payload, "TopRow") ?? ReadProperty(payload, "MensagemPadrao") ?? ScalarOrNull(payload)),
            "secondary_message" => Stringify(ReadProperty(payload, "MessageLine2") ?? ReadProperty(payload, "BottomRow") ?? ReadProperty(payload, "MensagemSecundaria") ?? ScalarOrNull(payload)),
            "release_duration" => Stringify(ReadProperty(payload, "ReleaseDuration") ?? ReadProperty(payload, "ReleaseTime") ?? ReadProperty(payload, "DuracaoAcionamento") ?? ScalarOrNull(payload)),
            "show_counters" => FormatBool(ReadProperty(payload, "ShowCounters") ?? ReadProperty(payload, "ExibirContador") ?? ScalarOrNull(payload)),
            "flow_mode" => FormatFlowMode(ReadProperty(payload, "ControlledFlow") ?? ReadProperty(payload, "ControleFluxo") ?? ReadProperty(payload, "In") ?? ScalarOrNull(payload)),
            "l3_flow_inverted" => FormatBool(ReadProperty(payload, "Inverted")),
            "l3_flow_out" => Stringify(ReadProperty(payload, "Out")),
            "l3_flow_front_wait" => Stringify(ReadProperty(payload, "FrontWait")),
            "l3_flow_picto_wait_in" => Stringify(ReadProperty(payload, "PictoWaitIn")),
            "l3_flow_picto_wait_out" => Stringify(ReadProperty(payload, "PictoWaitOut")),
            "ip_mode" => FormatIpMode(payload),
            "static_ip" => Stringify(ReadProperty(payload, "Ip") ?? ReadProperty(payload, "IP") ?? ReadProperty(payload, "ModoIP")),
            "subnet_mask" => Stringify(ReadProperty(payload, "Mask") ?? ReadProperty(payload, "SubnetMask") ?? ReadProperty(payload, "MascaraSubRede")),
            "gateway" => Stringify(ReadProperty(payload, "Gateway")),
            "entered" => Stringify(ReadProperty(payload, "Entered") ?? ReadProperty(payload, "ContadorHorario") ?? ScalarOrNull(payload)),
            "exited" => Stringify(ReadProperty(payload, "Exited") ?? ReadProperty(payload, "ContadorAntiHorario") ?? ScalarOrNull(payload)),
            _ => Stringify(payload)
        };
    }

    private static object? ReadProperty(object? value, string name)
    {
        if (value is null)
            return null;

        var property = value.GetType().GetProperty(name);
        return property?.GetValue(value);
    }

    private static string? Stringify(object? value) =>
        value switch
        {
            null => null,
            string text => string.IsNullOrWhiteSpace(text) ? null : text,
            JsonElement element => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),
            _ => value.ToString()
        };

    private static object? ScalarOrNull(object? value) =>
        value switch
        {
            null => null,
            string or bool or byte or short or int or long or float or double or decimal or Enum => value,
            JsonElement element when element.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element,
            _ => null
        };

    private static string? FormatBool(object? value) =>
        value switch
        {
            bool boolean => boolean ? "true" : "false",
            int number => number != 0 ? "true" : "false",
            _ => Stringify(value)
        };

    private static string? FormatMac(object? value) =>
        value switch
        {
            int[] bytes => string.Join(":", bytes.Select(item => item.ToString("X2"))),
            byte[] bytes => string.Join(":", bytes.Select(item => item.ToString("X2"))),
            _ => Stringify(value)
        };

    private static string? FormatIpMode(object? payload)
    {
        if (ReadProperty(payload, "StaticIp") is bool staticIp)
            return staticIp ? "Static" : "DHCP";

        var text = Stringify(ReadProperty(payload, "IpMode") ?? ReadProperty(payload, "ModoIP") ?? payload);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Contains("dhcp", StringComparison.OrdinalIgnoreCase) ? "DHCP" : text;
    }

    private static string? FormatFlowMode(object? value)
    {
        var text = Stringify(value);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.ToLowerInvariant() switch
        {
            "in" => "Entry",
            "out" => "Exit",
            "all" => "Both",
            _ => text
        };
    }

    private static bool TryGet(IReadOnlyDictionary<string, string> values, string key, out string value)
    {
        if (values.TryGetValue(key, out var candidate) && !string.IsNullOrWhiteSpace(candidate))
        {
            value = candidate.Trim();
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string? GetValue(IReadOnlyDictionary<string, string> values, string key) =>
        TryGet(values, key, out var value) ? value : null;

    private static bool TryGetInt(IReadOnlyDictionary<string, string> values, string key, out int value) =>
        int.TryParse(GetValue(values, key), out value);

    private static bool TryGetBool(IReadOnlyDictionary<string, string> values, string key, out bool value)
    {
        var text = GetValue(values, key);
        if (bool.TryParse(text, out value))
            return true;

        if (int.TryParse(text, out var number))
        {
            value = number != 0;
            return true;
        }

        value = false;
        return false;
    }

    private static bool IsDhcp(string ipMode) =>
        ipMode.Equals("DHCP", StringComparison.OrdinalIgnoreCase);

    private static ControlledFlow ToLiteNet2Flow(string value) =>
        Enum.TryParse<ControlledFlow>(value, ignoreCase: true, out var flow) ? flow : ControlledFlow.None;

    private static LiteNet1FlowMode ToLiteNet1Flow(string value) =>
        value.ToLowerInvariant() switch
        {
            "entry" => LiteNet1FlowMode.ControlaEntradaESaidaLiberada,
            "exit" => LiteNet1FlowMode.ControlaSaidaEEntradaLiberada,
            "both" => LiteNet1FlowMode.ControlaEntradaESaida,
            _ => LiteNet1FlowMode.Nunhum
        };

    private static string ToLiteNet3FlowDirection(string value) =>
        value.ToLowerInvariant() switch
        {
            "entry" or "in" => "In",
            "exit" or "out" => "Out",
            "both" or "all" => "All",
            _ => "None"
        };

    private static DeviceTypeKind ToUiType(HubDeviceType type) =>
        type switch
        {
            HubDeviceType.LiteNet1 => DeviceTypeKind.LiteNet1,
            HubDeviceType.LiteNet2 => DeviceTypeKind.LiteNet2,
            HubDeviceType.LiteNet3 => DeviceTypeKind.LiteNet3,
            HubDeviceType.SM25 => DeviceTypeKind.SM25,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    private static HubDeviceType ToHubType(DeviceTypeKind type) =>
        type switch
        {
            DeviceTypeKind.LiteNet1 => HubDeviceType.LiteNet1,
            DeviceTypeKind.LiteNet2 => HubDeviceType.LiteNet2,
            DeviceTypeKind.LiteNet3 => HubDeviceType.LiteNet3,
            DeviceTypeKind.SM25 => HubDeviceType.SM25,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}
