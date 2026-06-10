using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class DeviceDisplayStatusService
{
    public DeviceDisplayStatusViewModel GetStatus(
        DeviceRefViewModel device,
        string? currentCommandId = null,
        CommandHistoryItemViewModel? latestHistoryItem = null)
    {
        ArgumentNullException.ThrowIfNull(device);

        var status = ResolveStatus(device, currentCommandId, latestHistoryItem);
        return new DeviceDisplayStatusViewModel
        {
            Status = status,
            LabelKey = status switch
            {
                DeviceDisplayStatusKind.Connected => "Workspace.Connected",
                DeviceDisplayStatusKind.Disconnected => "Workspace.Disconnected",
                DeviceDisplayStatusKind.Offline => "Workspace.Disconnected",
                DeviceDisplayStatusKind.Connecting => "Device.Status.Connecting",
                DeviceDisplayStatusKind.Disconnecting => "Device.Status.Disconnecting",
                DeviceDisplayStatusKind.Busy => "Device.Status.Busy",
                DeviceDisplayStatusKind.Error => "Device.Status.Error",
                _ => "Workspace.Disconnected"
            },
            CssClass = status switch
            {
                DeviceDisplayStatusKind.Connected => "st-connected",
                DeviceDisplayStatusKind.Disconnected => "st-disconnected",
                DeviceDisplayStatusKind.Offline => "st-offline",
                DeviceDisplayStatusKind.Connecting => "st-connecting",
                DeviceDisplayStatusKind.Disconnecting => "st-disconnecting",
                DeviceDisplayStatusKind.Busy => "st-busy",
                DeviceDisplayStatusKind.Error => "st-error",
                _ => "st-offline"
            }
        };
    }

    private static DeviceDisplayStatusKind ResolveStatus(
        DeviceRefViewModel device,
        string? currentCommandId,
        CommandHistoryItemViewModel? latestHistoryItem)
    {
        if (latestHistoryItem?.Status == CommandStatusKind.Error &&
            string.Equals(latestHistoryItem.DeviceName, device.Name, StringComparison.Ordinal))
            return DeviceDisplayStatusKind.Error;

        if (string.Equals(currentCommandId, "connection.connect", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentCommandId, "connection.connect_serial", StringComparison.OrdinalIgnoreCase))
            return DeviceDisplayStatusKind.Connecting;

        if (string.Equals(currentCommandId, "connection.disconnect", StringComparison.OrdinalIgnoreCase))
            return DeviceDisplayStatusKind.Disconnecting;

        if (!string.IsNullOrWhiteSpace(currentCommandId))
            return DeviceDisplayStatusKind.Busy;

        if (device.Connected)
            return DeviceDisplayStatusKind.Connected;

        return string.IsNullOrWhiteSpace(device.IpAddress)
            ? DeviceDisplayStatusKind.Offline
            : DeviceDisplayStatusKind.Disconnected;
    }
}
