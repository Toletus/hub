using Toletus.Hub.Models;

namespace Toletus.Hub.Notifications;

public class Notifier
{
    private static readonly List<Notification> Notifications = [];

    public static void AddNotification(string ip, int id, int command, DeviceType type)
    {
        if (HasNotification(ip, command)) return;

        Notifications.Add(new Notification(ip, id, command, type));
    }

    public static void UpdateNotification(string ip, int command, DeviceResponse deviceResponse)
    {
        Notifications.FirstOrDefault(x => x.Ip == ip && x.Command == command)!.Response = deviceResponse;
    }

    public static bool HasNotification(string ip, int command, bool hasResponse = true)
    {
        return hasResponse
            ? Notifications.Exists(x => x.Ip == ip && x.Command == command && x.Response != null)
            : Notifications.Exists(x => x.Ip == ip && x.Command == command);
    }

    public static Notification GetNotification(string ip, int command)
    {
        return Notifications.FirstOrDefault(x => x.Ip == ip && x.Command == command)!;
    }

    public static void ClearNotification(string ip, int command)
    {
        if (!HasNotification(ip, command)) return;

        Notifications.Remove(GetNotification(ip, command));
    }
}