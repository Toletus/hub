using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Helpers;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices.Base;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Base;
using Toletus.LiteNet2.Command;
using Toletus.LiteNet2.Command.Enums;
using Toletus.LiteNet2.Enums;

namespace Toletus.Hub.Services.NotificationsServices;

public class LiteNet2NotificationService : NotificationBaseService
{
    public static void Initialize()
    {
        LiteNet2Devices.OnBoardReceived += OnLiteNet2BoardReceived;
    }

    protected static async Task<Notification> ExecuteCommandAsync(
        Device device,
        LiteNet2Commands command,
        Action? deviceAction)
    {
        device = RefreshDeviceConnectionState(device);
        if (TryCreateDeviceActionNotification(deviceAction, device, (int)command, out var notification))
            return notification;

        Notifier.AddNotification(device.Ip, device.Id, (int)command, device.Type);
        deviceAction!.Invoke();
        return await Notification.GetNotification(device.Ip, device.Id, (int)command, device.Type);
    }

    protected static async Task<Notification> ExecuteCommandAsync(
        Device device,
        LiteNet2Commands command,
        DeviceResponse deviceResponse,
        Action? deviceAction)
    {
        device = RefreshDeviceConnectionState(device);
        if (TryCreateDeviceActionNotification(deviceAction, device, (int)command, out var notification))
            return notification;

        deviceAction!.Invoke();
        return await Notification.GetNotification(device.Ip, device.Id, (int)command, device.Type, deviceResponse);
    }

    public static void OnLiteNet2BoardReceived(LiteNet2Board obj)
    {
        obj.OnResponse += Board_OnResponse;
        obj.OnIdentification += Board_OnIdentification;
        obj.OnGyre += Board_OnGyre;
        obj.OnStatus += Board_OnStatus;
        obj.OnReady += Board_OnReady;
        obj.OnConnectionStatusChanged += Board_OnConnectionStatusChanged;
        obj.OnFingerprintReaderConnected += Board_OnFingerprintReaderConnected;
    }

    private static void Board_OnResponse(LiteNet2Board litenet2Board, LiteNet2Response litenet2Response)
    {
        var ip = litenet2Board.Ip.ToString();
        var command = (int)litenet2Response.Command;

        var response = GetResponseOrNotification(litenet2Board, litenet2Response, out var notification);

        ProcessNotification(ip, command, response, notification, shouldSendToDelegate: false);
    }

    private static void Board_OnIdentification(LiteNet2BoardBase litenet2Board, Identification identification)
    {
        var command = identification.Device switch
        {
            IdentificationDevice.Keyboard => LiteNet2Commands.IdentificationByKeyboard,
            IdentificationDevice.BarCode => LiteNet2Commands.IdentificationByBarCode,
            IdentificationDevice.Rfid => LiteNet2Commands.IdentificationByRfId,
            IdentificationDevice.EmbeddedFingerprint => identification.Data > 0
                ? LiteNet2Commands.PositiveIdentificationByFingerprintReader
                : LiteNet2Commands.NegativeIdentificationByFingerprintReader,
            _ => LiteNet2Commands.NegativeIdentificationByFingerprintReader
        };

        var notification = Notification.CreateNotification(
            litenet2Board.Ip.ToString(),
            litenet2Board.Id,
            (int)command,
            DeviceType.LiteNet2,
            new { Identification = identification });

        ProcessNotification(litenet2Board.Ip.ToString(), (int)command, notification: notification, shouldSendToWebhook: false);
    }

    private static void Board_OnGyre(LiteNet2Board litenet2Board, Direction direction)
    {
        var command = direction != Direction.None ? (int)LiteNet2Commands.Gyre : (int)LiteNet2Commands.GyreTimeout;

        var notification = Notification.CreateNotification(
            litenet2Board.Ip.ToString(),
            litenet2Board.Id,
            command,
            DeviceType.LiteNet2,
            new { Gyre = direction });

        ProcessNotification(litenet2Board.Ip.ToString(), command, notification: notification, shouldSendToWebhook: false);
    }

    private static void Board_OnStatus(LiteNet2BoardBase litenet2Board, string status)
    {
        var notification = Notification.CreateNotification(
            litenet2Board.Ip.ToString(),
            litenet2Board.Id,
            0,
            DeviceType.LiteNet2,
            new { Status = status });

        ProcessNotification(litenet2Board.Ip.ToString(), 0, notification: notification, shouldSendToWebhook: false);
    }

    private static void Board_OnReady(LiteNet2Board litenet2Board, bool isReady)
    {
        var notification = Notification.CreateNotification(
            litenet2Board.Ip.ToString(),
            litenet2Board.Id,
            0,
            DeviceType.LiteNet2,
            new { IsReady = isReady });

        ProcessNotification(litenet2Board.Ip.ToString(), 0, notification: notification, shouldSendToWebhook: false);
    }

    private static void Board_OnConnectionStatusChanged(LiteNet2BoardBase litenet2Board,
        BoardConnectionStatus boardConnectionStatus)
    {
        var notification = Notification.CreateNotification(
            litenet2Board.Ip.ToString(),
            litenet2Board.Id,
            0,
            DeviceType.LiteNet2,
            new { BoardConnectionStatus = boardConnectionStatus });

        ProcessNotification(litenet2Board.Ip.ToString(), 0, notification: notification, shouldSendToWebhook: false);
    }

    private static void Board_OnFingerprintReaderConnected(LiteNet2Board litenet2Board, bool fingerprintReaderConnected)
    {
        var notification = Notification.CreateNotification(
            litenet2Board.Ip.ToString(),
            litenet2Board.Id,
            0,
            DeviceType.LiteNet2,
            new { FingerprintReaderConnected = fingerprintReaderConnected });

        ProcessNotification(litenet2Board.Ip.ToString(), 0, notification: notification, shouldSendToWebhook: false);
    }

    private static DeviceResponse? GetResponseOrNotification(
        LiteNet2Board litenet2Board,
        LiteNet2Response litenet2Response,
        out Notification? notification)
    {
        notification = null;
        if ((int)litenet2Response.Command < (int)LiteNet2Commands.IdentificationByRfId)
            return GetDeviceResponse(litenet2Board, litenet2Response);

        notification = Notification.GetNotification(litenet2Board, litenet2Response);
        return null;
    }

    private static DeviceResponse GetDeviceResponse(LiteNet2Board liteNet2Board, LiteNet2Response liteNet2Response)
    {
        var commandMap = CommandHelper.CreateCommandMap<LiteNet2Board, object>();

        if (commandMap.TryGetValue(liteNet2Response.Command, out var dataSelector))
            return new DeviceResponse(GetResponseData(dataSelector));

        return liteNet2Response.Command == LiteNet2Commands.GetCounters
            ? new DeviceResponse(new { Content = liteNet2Response.Data })
            : new DeviceResponse(liteNet2Response);

        object GetResponseData(Func<LiteNet2Board, object> dataExtractor)
        {
            return new { Content = dataExtractor(liteNet2Board) };
        }
    }
}