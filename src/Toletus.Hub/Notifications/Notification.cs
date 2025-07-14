using Toletus.Hub.Helpers;
using Toletus.Hub.Models;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Command;

namespace Toletus.Hub.Notifications;

public class Notification(string ip, int id, int command, DeviceType deviceType, object? response = null)
{
    private const int DefaultPollingIntervalMilliseconds = 2;
    private const int MaxPollingDurationSeconds = 30;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(DefaultPollingIntervalMilliseconds);
    private static readonly TimeSpan MaxPollingDuration = TimeSpan.FromSeconds(MaxPollingDurationSeconds);

    public string Ip { get; set; } = ip;
    public int Id { get; set; } = id;
    public int Command { get; set; } = command;
    public DeviceType Type { get; set; } = deviceType;
    public object? Response { get; set; } = response;

    public static async Task<Notification> GetNotification(string ip, int id, int command, DeviceType type)
    {
        var timer = new PeriodicTimer(PollingInterval);
        var startTime = DateTime.UtcNow;

        do
        {
            if (Notifier.HasNotification(ip, command))
                return ExtractAndClearNotification(ip, command);

            if (DateTime.UtcNow - startTime > MaxPollingDuration)
                break;
        } while (await timer.WaitForNextTickAsync());

        return CreateTimeoutNotification(ip, id, command, type);
    }

    public static Notification GetNotification(LiteNet2Board liteNet2Board, LiteNet2Response liteNet2Response)
    {
        var ip = liteNet2Board.Ip.ToString();
        var id = liteNet2Board.Id;
        var command = (int)liteNet2Response.Command;

        var commandMap =
            CommandHelper.CreateCommandMap<LiteNet2Response, Notification>(ip, id, command, DeviceType.LiteNet2);

        return commandMap.TryGetValue(liteNet2Response.Command, out var notificationFactory)
            ? notificationFactory(liteNet2Response)
            : new Notification(ip, id, command, DeviceType.LiteNet2, liteNet2Response);
    }

    public static Notification CreateNotification(string ip, int id, int command, DeviceType type, object response) =>
        new(ip, id, command, type, response);

    public static Task<Notification>
        GetNotification(string ip, int id, int command, DeviceType type, DeviceResponse response) =>
        Task.FromResult(CreateNotification(ip, id, command, type, response));

    private static Notification ExtractAndClearNotification(string ip, int command)
    {
        var notification = Notifier.GetNotification(ip, command);
        Notifier.ClearNotification(ip, command);
        return notification;
    }

    private static Notification CreateTimeoutNotification(string ip, int id, int command, DeviceType type)
    {
        Notifier.ClearNotification(ip, command);
        return CreateNotification(ip, id, command, type, new DeviceResponse(false, "The request has timed out"));
    }
}