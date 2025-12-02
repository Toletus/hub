using Toletus.Hub.Models;

namespace Toletus.Hub.Notifications;

public static class Notifier
{
    private static readonly Lock NotificationsLock = new();
    private static readonly List<Notification> Notifications = [];

    public static void AddNotification(string ip, int id, int command, DeviceType type)
    {
        lock (NotificationsLock)
        {
            if (HasNotification(ip, command, hasResponse: false))
                return;

            Notifications.Add(new Notification(ip, id, command, type));
        }
    }

    public static void UpdateNotification(string ip, int command, DeviceResponse deviceResponse)
    {
        lock (NotificationsLock)
        {
            Notifications.FirstOrDefault(x => x.Ip == ip && x.Command == command)!.Response = deviceResponse;
        }
    }

    public static bool HasNotification(string ip, int command, bool hasResponse = true)
    {
        lock (NotificationsLock)
        {
            return hasResponse
                ? Notifications.Exists(x =>
                    x != null && x.Ip == ip && x.Command == command && x.Response != null)
                : Notifications.Exists(x =>
                    x != null && x.Ip == ip && x.Command == command);
        }
    }

    public static Notification GetNotification(string ip, int command)
    {
        lock (NotificationsLock)
        {
            return Notifications.FirstOrDefault(x => x.Ip == ip && x.Command == command)!;
        }
    }

    public static void ClearNotification(string ip, int command)
    {
        lock (NotificationsLock)
        {
            var notification = Notifications.FirstOrDefault(x => x?.Ip == ip && x.Command == command);
            if (notification != null)
                Notifications.Remove(notification);
        }
    }
}