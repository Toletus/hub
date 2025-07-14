using System.Net;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Command.Enums;

namespace Toletus.Hub.Services;

public class LiteNet2CommandService : LiteNet2NotificationService
{
    #region Reads Commands

    public async Task<Notification> GetBuzzerMute(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetBuzzerMute,
            () => device.Get<LiteNet2Board>()?.GetBuzzerMute());
    }

    public async Task<Notification> GetCounters(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetCounters,
            () => device.Get<LiteNet2Board>()?.GetCounters());
    }

    public async Task<Notification> GetEntryClockwise(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetEntryClockwise,
            () => device.Get<LiteNet2Board>()?.GetEntryClockwise());
    }

    public async Task<Notification> GetFingerprintIdentificationMode(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetFingerprintIdentificationMode,
            () => device.Get<LiteNet2Board>()?.GetFingerprintIdentificationMode());
    }

    public async Task<Notification> GetFirmwareVersion(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetFirmwareVersion,
            () => device.Get<LiteNet2Board>()?.GetFirmwareVersion());
    }

    public async Task<Notification> GetFlowControl(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetFlowControl,
            () => device.Get<LiteNet2Board>()?.GetFlowControl());
    }

    public async Task<Notification> GetFlowControlExtended(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetFlowControlExtended,
            () => device.Get<LiteNet2Board>()?.GetFlowControlExtended());
    }

    public async Task<Notification> GetId(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetId,
            () => device.Get<LiteNet2Board>()?.GetId());
    }

    public async Task<Notification> GetIpMode(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetIpMode,
            () => device.Get<LiteNet2Board>()?.GetIpMode());
    }

    public async Task<Notification> GetMac(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetMac,
            () => device.Get<LiteNet2Board>()?.GetMac());
    }

    public async Task<Notification> GetMenuPassword(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetMenuPassword,
            () => device.Get<LiteNet2Board>()?.GetMenuPassword());
    }

    public async Task<Notification> GetMessageLine1(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetMessageLine1,
            () => device.Get<LiteNet2Board>()?.GetMessageLine1());
    }

    public async Task<Notification> GetMessageLine2(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetMessageLine2,
            () => device.Get<LiteNet2Board>()?.GetMessageLine2());
    }

    public async Task<Notification> GetReleaseDuration(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetReleaseDuration,
            () => device.Get<LiteNet2Board>()?.GetReleaseDuration());
    }

    public async Task<Notification> GetSerialNumber(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetSerialNumber,
            () => device.Get<LiteNet2Board>()?.GetSerialNumber());
    }

    public async Task<Notification> GetShowCounters(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.GetShowCounters,
            () => device.Get<LiteNet2Board>()?.GetShowCounters());
    }

    #endregion

    #region Writes Commands

    public async Task<Notification> Notify(
        Device device,
        int duration = 1000,
        int tone = 0,
        int color = 0,
        int showMessage = 0)
    {
        var parameter = new[]
        {
            (byte)(duration & 0xFF), (byte)((duration >> 8) & 0xFF), // Duração em 16 bits
            (byte)tone, // Toque em 8 bits
            (byte)color, // Cor em 8 bits
            (byte)showMessage // Mostrar texto em 8 bits
        };

        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.Notify,
            new DeviceResponse(true, "Notification sent successfully."),
            () => device.Get<LiteNet2Board>()?.Notify(parameter));
    }

    public async Task<Notification> ReleaseEntry(Device device, string message)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.ReleaseEntry,
            new DeviceResponse(true, "Entry released successfully."),
            () => device.Get<LiteNet2Board>()?.ReleaseEntry(message));
    }

    public async Task<Notification> ReleaseEntryAndExit(Device device, string message)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.ReleaseEntryAndExit,
            new DeviceResponse(true, "Entry and exit released successfully."),
            () => device.Get<LiteNet2Board>()?.ReleaseEntryAndExit(message));
    }

    public async Task<Notification> ReleaseExit(Device device, string message)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.ReleaseExit,
            new DeviceResponse(true, "Exit released successfully."),
            () => device.Get<LiteNet2Board>()?.ReleaseExit(message));
    }

    public async Task<Notification> Reset(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.Reset,
            new DeviceResponse(true, "Device reset successfully."),
            () => device.Get<LiteNet2Board>()?.Reset());
    }

    public async Task<Notification> ResetCounters(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.ResetCounters,
            new DeviceResponse(true, "Counters reset successfully."),
            () => device.Get<LiteNet2Board>()?.ResetCounters());
    }

    public async Task<Notification> ResetPeripherals(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.ResetPeripherals,
            new DeviceResponse(true, "Peripherals reset successfully."),
            () => device.Get<LiteNet2Board>()?.ResetPeripherals());
    }

    public async Task<Notification> SetId(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetId,
            new DeviceResponse(true, "Device ID set successfully."),
            () => device.Get<LiteNet2Board>()?.SetId(device.Id));
    }

    public async Task<Notification> SetIp(Device device, bool dhcp, string ip, string subnetMask)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetIp,
            new DeviceResponse(true, "IP address set successfully."),
            () => device.Get<LiteNet2Board>()?.SetIp(dhcp, IPAddress.Parse(ip), IPAddress.Parse(subnetMask)));
    }

    public async Task<Notification> SetBuzzerMute(Device device, bool on)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetBuzzerMute,
            new DeviceResponse(true, "Buzzer mute set successfully."),
            () => device.Get<LiteNet2Board>()?.SetBuzzerMute(on));
    }

    public async Task<Notification> SetEntryClockwise(Device device, bool entryClockwise)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetEntryClockwise,
            new DeviceResponse(true, "Entry clockwise set successfully."),
            () => device.Get<LiteNet2Board>()?.SetEntryClockwise(entryClockwise));
    }

    public async Task<Notification> SetFlowControl(Device device, ControlledFlow controlledFlow)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetFlowControl,
            new DeviceResponse(true, "Flow control set successfully."),
            () => device.Get<LiteNet2Board>()?.SetFlowControl(controlledFlow));
    }

    public async Task<Notification> SetFingerprintIdentificationMode(Device device,
        FingerprintIdentificationMode fingerprintIdentificationMode)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetFingerprintIdentificationMode,
            new DeviceResponse(true, "Fingerprint identification mode set successfully."),
            () => device.Get<LiteNet2Board>()?.SetFingerprintIdentificationMode(fingerprintIdentificationMode));
    }

    public async Task<Notification> SetFlowControlExtended(Device device, ControlledFlowExtended controlledFlowExtended)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetFlowControlExtended,
            new DeviceResponse(true, "Extended flow control set successfully."),
            () => device.Get<LiteNet2Board>()?.SetFlowControlExtended(controlledFlowExtended));
    }

    public async Task<Notification> SetMac(Device device, string mac)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetMac,
            new DeviceResponse(true, "MAC address set successfully."),
            () => device.Get<LiteNet2Board>()?.SetMac(mac));
    }

    public async Task<Notification> SetMenuPassword(Device device, string password)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetMenuPassword,
            new DeviceResponse(true, "Menu password set successfully."),
            () => device.Get<LiteNet2Board>()?.SetMenuPassword(password));
    }

    public async Task<Notification> SetMessageLine1(Device device, string message)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetMessageLine1,
            new DeviceResponse(true, "Message line 1 set successfully."),
            () => device.Get<LiteNet2Board>()?.SetMessageLine1(message));
    }

    public async Task<Notification> SetMessageLine2(Device device, string message)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetMessageLine2,
            new DeviceResponse(true, "Message line 2 set successfully."),
            () => device.Get<LiteNet2Board>()?.SetMessageLine2(message));
    }

    public async Task<Notification> SetReleaseDuration(Device device, int releaseDuration)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetReleaseDuration,
            new DeviceResponse(true, "Release duration set successfully."),
            () => device.Get<LiteNet2Board>()?.SetReleaseDuration(releaseDuration));
    }

    public async Task<Notification> SetShowCounters(Device device, bool showCounters)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.SetShowCounters,
            new DeviceResponse(true, "Show counters set successfully."),
            () => device.Get<LiteNet2Board>()?.SetShowCounters(showCounters));
    }

    public async Task<Notification> TempMessage(Device device, string message)
    {
        return await ExecuteCommandAsync(
            device,
            LiteNet2Commands.TempMessage,
            new DeviceResponse(true, "Temporary message sent successfully."),
            () => device.Get<LiteNet2Board>()?.TempMessage(message));
    }

    #endregion
}