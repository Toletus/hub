using Toletus.Hub.Models;
using Toletus.Hub.Notifications;

namespace Toletus.Hub.Services.NotificationsServices.Base;

public class NotificationBaseService
{
    public static Action<Notification>? OnNotification;

    protected const string DeviceActionNullMessage =
        "Unable to execute the action: device is not in connected. Check if it is registered and accessible.";

    protected static void ProcessNotification(
        string ip,
        int command,
        DeviceResponse? response = null,
        Notification? notification = null,
        bool shouldSendToDelegate = true,
        bool shouldSendToWebhook = true)
    {
        if (shouldSendToDelegate && notification != null)
            OnNotification?.Invoke(notification);

        HandleNotification(ip, command, response, notification, shouldSendToWebhook);
    }

    protected static Device RefreshDeviceConnectionState(Device device)
    {
        device.Connected = DeviceService.Devices?.FirstOrDefault(x =>
            x.Ip == device.Ip
            && x.Type == device.Type)?.Connected ?? false;

        return device;
    }

    private static void HandleNotification(
        string ip,
        int command,
        DeviceResponse? response,
        Notification? notification,
        bool shouldSendToWebhook)
    {
        if (Notifier.HasNotification(ip, command, hasResponse: false))
            Notifier.UpdateNotification(ip, command, response!);
        else if (notification != null && shouldSendToWebhook)
            _ = WebhookSerivce.PostDeviceResponse(notification);
    }

    protected static bool TryCreateDeviceActionNotification(
        Action? deviceAction,
        Device device,
        int command,
        out Notification notification)
    {
        if (CheckDeviceActionValidity(deviceAction, device))
        {
            notification = Notification.CreateNotification(
                device.Ip,
                device.Id,
                command,
                device.Type,
                new DeviceResponse(false, DeviceActionNullMessage));
            return true;
        }

        notification = null!;
        return false;
    }

    private static bool CheckDeviceActionValidity(Action? deviceAction, Device device) =>
        !device.Connected || deviceAction == null;
}