using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Helpers;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices.Base;
using Toletus.LiteNet3;
using Toletus.LiteNet3.Handler.Responses.Base;
using Toletus.LiteNet3.Handler.Responses.NotificationsResponses;
using Toletus.LiteNet3.Handler.Responses.NotificationsResponses.Base;

namespace Toletus.Hub.Services.NotificationsServices;

public class LiteNet3NotificationService : NotificationBaseService
{
    public static void Initialize()
    {
        LiteNet3Devices.OnBoardReceived += OnLiteNet3BoardReceived;
    }

    protected static async Task<Notification> ExecuteCommandAsync(
        Device device,
        ResponseType command,
        Action? deviceAction,
        bool waitResponse = true)
    {
        device = RefreshDeviceConnectionState(device);
        if (TryCreateDeviceActionNotification(deviceAction, device, (int)command, out var notification))
            return notification;

        Notifier.AddNotification(device.Ip, device.Id, (int)command, device.Type);
        deviceAction!.Invoke();
        if (waitResponse)
            return await Notification.GetNotification(device.Ip, device.Id, (int)command, device.Type);

        return Notification.CreateNotification(
            device.Ip,
            device.Id,
            (int)command, device.Type,
            new DeviceResponse(true));
    }

    public static void OnLiteNet3BoardReceived(LiteNet3Board obj)
    {
        obj.OnResponse += Board_OnResponse;
        obj.OnResult += Board_OnResult;
        obj.OnIdentification += Board_OnIdentification;
        obj.OnBiometricsResponse += Board_OnBiometricsResponse;
        obj.OnReleaseResponse += Board_OnReleaseResponse;
    }

    private static void Board_OnResponse(LiteNet3BoardBase board, object response)
    {
        var command = CommandHelper.GetCommand(response);

        ProcessNotification(
            board.Ip.ToString(),
            command,
            new DeviceResponse(response));
    }

    private static void Board_OnResult(LiteNet3BoardBase board, ResultBase result)
    {
        var command = CommandHelper.GetCommand(result);

        var message = result.Reason;
        var success = result.Result == "ok";

        ProcessNotification(
            board.Ip.ToString(),
            command,
            new DeviceResponse(success, message));
    }

    private static void Board_OnIdentification(LiteNet3BoardBase boardBase, CodeBase codeBase)
    {
        var command = codeBase switch
        {
            RfidResponse => (int)ResponseType.Rfid,
            BarcodeResponse => (int)ResponseType.Barcode,
            KeypadResponse => (int)ResponseType.Keypad
        };

        var notification = Notification.CreateNotification(
            boardBase.Ip.ToString(),
            boardBase.Id,
            command,
            DeviceType.LiteNet3,
            new { Identification = codeBase });

        ProcessNotification(boardBase.Ip.ToString(), command, null, notification);
    }

    private static void Board_OnBiometricsResponse(LiteNet3BoardBase boardBase, byte[] biometricPayload)
    {
        const int command = (int)ResponseType.Biometrics;

        var notification = Notification.CreateNotification(
            boardBase.Ip.ToString(),
            boardBase.Id,
            command,
            DeviceType.LiteNet3,
            new { Biometrics = biometricPayload });

        ProcessNotification(boardBase.Ip.ToString(), command, null, notification);
    }

    private static void Board_OnReleaseResponse(LiteNet3BoardBase boardBase, ReleaseBase releaseBase)
    {
        var ipAddress = boardBase.Ip.ToString();

        var (command, payload) = releaseBase switch
        {
            PassageResponse passageResponse => ((int)ResponseType.Passage, (object)new { Passage = passageResponse }),
            TimeoutResponse timeoutResponse => ((int)ResponseType.Timeout, (object)new { Timeout = timeoutResponse })
        };

        var notification = Notification.CreateNotification(
            ipAddress,
            boardBase.Id,
            command,
            DeviceType.LiteNet3,
            payload);

        ProcessNotification(ipAddress, command, null, notification);
    }
}