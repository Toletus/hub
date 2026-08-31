using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Xunit;

namespace Toletus.Hub.Tests;

// T-021 — correlação por token (FIFO) e resposta órfã sem exceção.
public class NotifierCorrelationTests
{
    // Dois comandos iguais concorrentes recebem cada um a própria resposta (FIFO).
    [Fact]
    public void Identical_concurrent_commands_each_get_their_own_response()
    {
        const string ip = "10.0.0.1";
        const int command = 100;

        var t1 = Notifier.AddNotification(ip, 1, command, DeviceType.LiteNet2);
        var t2 = Notifier.AddNotification(ip, 1, command, DeviceType.LiteNet2);

        var first = new DeviceResponse(true, "first");
        var second = new DeviceResponse(true, "second");
        Notifier.CompleteOldest(ip, command, first);
        Notifier.CompleteOldest(ip, command, second);

        Assert.True(Notifier.TryTakeResponse(t1, out var r1));
        Assert.True(Notifier.TryTakeResponse(t2, out var r2));
        Assert.Same(first, r1);   // o token mais antigo pega a 1ª resposta
        Assert.Same(second, r2);
    }

    // Resposta sem dono: registrada e descartada, nunca lança (corrige o NPE da corrida antiga).
    [Fact]
    public void Orphan_response_is_discarded_without_throwing()
    {
        const string ip = "10.0.0.2";
        var logged = new List<string>();
        Notifier.Log = logged.Add;

        var ex = Record.Exception(() =>
            Notifier.CompleteOldest(ip, 200, new DeviceResponse(true, "orphan")));

        Assert.Null(ex);
        Assert.Contains(logged, l => l.Contains("sem dono"));
        Notifier.Log = null;
    }

    // Uma resposta é consumida uma única vez.
    [Fact]
    public void Response_is_taken_once()
    {
        const string ip = "10.0.0.3";
        const int command = 300;

        var token = Notifier.AddNotification(ip, 1, command, DeviceType.LiteNet2);
        Notifier.CompleteOldest(ip, command, new DeviceResponse(true, "x"));

        Assert.True(Notifier.TryTakeResponse(token, out _));
        Assert.False(Notifier.TryTakeResponse(token, out _)); // já removida
    }

    [Fact]
    public void Pending_without_response_is_not_takeable()
    {
        const string ip = "10.0.0.4";
        var token = Notifier.AddNotification(ip, 1, 400, DeviceType.LiteNet2);

        Assert.False(Notifier.TryTakeResponse(token, out _));
        Assert.True(Notifier.HasPending(ip, 400));
        Notifier.Remove(token);
        Assert.False(Notifier.HasPending(ip, 400));
    }
}
