using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices.Base;
using Toletus.LiteNet1;
using Toletus.LiteNet1.Enums;

namespace Toletus.Hub.Services.NotificationsServices;

public class LiteNet1NotificationService : NotificationBaseService
{
    public static void Initialize()
    {
        LiteNet1Devices.OnBoardReceived += OnLiteNet1BoardReceived;
    }

    protected static async Task<Notification> ExecuteCommandWithResultAsync<TParams>(
        Device device,
        Func<TParams, object>? deviceAction,
        TParams parameters)
    {
        device = RefreshDeviceConnectionState(device);
        if (TryCreateDeviceActionNotification(deviceAction, device, 0, out var notification))
            return notification;

        var result = deviceAction!(parameters);
        return Notification.CreateNotification(device.Ip, device.Id, 0, device.Type, result);
    }

    public static void OnLiteNet1BoardReceived(LiteNet obj, Controlador controlador)
    {
        controlador.OnIdentificacao += Board_OnIdentification;
        controlador.OnAcessou += Board_OnGyre;
    }

    private static void Board_OnIdentification(LiteNet sender, Identificacao identificacao)
    {
        var notification = Notification.CreateNotification(
            sender.IP,
            sender.Id,
            (int)identificacao.DispositivoIdentificacao,
            DeviceType.LiteNet1,
            new { Identification = identificacao });

        ProcessNotification(sender.IP, (int)identificacao.DispositivoIdentificacao, notification: notification);
    }

    private static void Board_OnGyre(LiteNet sender, bool sucesso, Sentido sentido)
    {
        if (!sucesso)
            sentido = Sentido.Nenhum;

        var notification = Notification.CreateNotification(
            sender.IP,
            sender.Id,
            (int)sentido,
            DeviceType.LiteNet1,
            new { GyreResponse = sentido });

        ProcessNotification(sender.IP, (int)sentido, notification: notification);
    }

    private static bool TryCreateDeviceActionNotification<TParams>(
        Func<TParams, object>? deviceAction,
        Device device,
        int command,
        out Notification notification)
    {
        if (!device.Connected || deviceAction == null)
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
}