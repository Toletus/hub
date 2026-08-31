using Toletus.Hub.Models;

namespace Toletus.Hub.Notifications;

/// <summary>
/// Correlaciona resposta↔requisição por token (TDD §4.4). Vários comandos iguais concorrentes
/// à mesma catraca coexistem (fila FIFO por ip+comando); a resposta completa o pedido mais antigo.
/// Resposta sem dono é registrada e descartada — nunca lança (corrige a corrida que fechava a conexão).
/// </summary>
public static class Notifier
{
    public static Action<string>? Log;

    private static readonly Lock NotificationsLock = new();
    private static readonly List<Notification> Notifications = [];

    /// <summary>Registra um pedido pendente e devolve o token para o chamador aguardar a própria resposta.</summary>
    public static Guid AddNotification(string ip, int id, int command, DeviceType type)
    {
        var notification = new Notification(ip, id, command, type);
        lock (NotificationsLock)
            Notifications.Add(notification);
        return notification.Token;
    }

    /// <summary>Completa o pedido pendente mais antigo de (ip, comando). Órfã: registra e descarta.</summary>
    public static void CompleteOldest(string ip, int command, DeviceResponse deviceResponse)
    {
        lock (NotificationsLock)
        {
            var pending = Notifications
                .Where(x => x is { } && x.Ip == ip && x.Command == command && x.Response == null)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefault();

            if (pending == null)
            {
                Log?.Invoke($"[Notifier] Resposta sem dono descartada (ip {ip}, comando {command}).");
                return;
            }

            pending.Response = deviceResponse;
        }
    }

    public static bool TryTakeResponse(Guid token, out DeviceResponse? response)
    {
        lock (NotificationsLock)
        {
            var notification = Notifications.FirstOrDefault(x => x.Token == token);
            if (notification?.Response is DeviceResponse dr)
            {
                Notifications.Remove(notification);
                response = dr;
                return true;
            }

            response = null;
            return false;
        }
    }

    public static void Remove(Guid token)
    {
        lock (NotificationsLock)
        {
            var notification = Notifications.FirstOrDefault(x => x.Token == token);
            if (notification != null)
                Notifications.Remove(notification);
        }
    }

    public static bool HasPending(string ip, int command)
    {
        lock (NotificationsLock)
            return Notifications.Exists(x => x is { } && x.Ip == ip && x.Command == command && x.Response == null);
    }
}
