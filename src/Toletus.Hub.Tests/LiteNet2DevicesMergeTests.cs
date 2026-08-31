using System.Net;
using Toletus.Hub.DeviceCollectionManager;
using Toletus.LiteNet2;
using Xunit;

namespace Toletus.Hub.Tests;

// T-020 / ADR-002 — mesclagem não destrutiva do registro (TDD §4.4), seis casos + concorrência.
public class LiteNet2DevicesMergeTests : IDisposable
{
    private readonly LoopbackServer _server = new();

    public LiteNet2DevicesMergeTests() => LiteNet2Devices.Clear();

    public void Dispose()
    {
        LiteNet2Devices.Clear();
        _server.Dispose();
    }

    private LiteNet2Board Connected(string ip, string serial, int id)
    {
        var b = new LiteNet2Board(IPAddress.Parse(ip), serial, id) { ConnectPort = _server.Port };
        b.Connect();
        Assert.True(b.Connected);
        return b;
    }

    private static LiteNet2Board Disconnected(string ip, string serial, int id)
        => new(IPAddress.Parse(ip), serial, id);

    // Caso 1 — identidade conhecida, conectada: mantém a instância e atualiza o endereço in place.
    [Fact]
    public void Known_connected_keeps_instance_and_updates_address()
    {
        var connected = Connected("127.0.0.2", "S1", 1);
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { connected });

        var received = 0;
        LiteNet2Devices.OnBoardReceived += _ => received++;

        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { Disconnected("127.0.0.9", "S1", 1) });

        var boards = LiteNet2Devices.Boards!;
        Assert.Single(boards);
        Assert.Same(connected, boards[0]);                    // mesma instância conectada
        Assert.Equal("127.0.0.9", boards[0].Ip.ToString());   // endereço atualizado
        Assert.Equal(0, received);                            // não re-dispara OnBoardReceived (AC-007.2/007.3)
        LiteNet2Devices.OnBoardReceived = null;
    }

    // Caso 2 — identidade conhecida, desconectada: substitui pela nova instância.
    [Fact]
    public void Known_disconnected_is_replaced()
    {
        var old = Disconnected("127.0.0.3", "S2", 2);
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { old });

        var fresh = Disconnected("127.0.0.3", "S2", 2);
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { fresh });

        var boards = LiteNet2Devices.Boards!;
        Assert.Single(boards);
        Assert.Same(fresh, boards[0]);
    }

    // Caso 3 — identidade nova: adiciona.
    [Fact]
    public void New_identity_is_added()
    {
        LiteNet2Devices.SetBoards(new List<LiteNet2Board>
        {
            Disconnected("127.0.0.4", "S3", 3),
            Disconnected("127.0.0.5", "S4", 4)
        });

        Assert.Equal(2, LiteNet2Devices.Boards!.Count);
    }

    // Caso 4 — ausente da varredura, desconectada: remove.
    [Fact]
    public void Absent_disconnected_is_removed()
    {
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { Disconnected("127.0.0.4", "S5", 5) });
        LiteNet2Devices.SetBoards(new List<LiteNet2Board>()); // varredura vazia

        Assert.Empty(LiteNet2Devices.Boards!);
    }

    // Caso 5 — ausente da varredura, conectada: mantém (o transporte decide a queda).
    [Fact]
    public void Absent_connected_is_kept()
    {
        var connected = Connected("127.0.0.6", "S6", 6);
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { connected });

        LiteNet2Devices.SetBoards(new List<LiteNet2Board>()); // varredura vazia

        Assert.Single(LiteNet2Devices.Boards!);
        Assert.Same(connected, LiteNet2Devices.Boards![0]);
    }

    // Caso 6 — mesmo endereço, identidade divergente, conectada: mantém a conectada + log de divergência.
    [Fact]
    public void Same_address_divergent_identity_keeps_connected_and_logs()
    {
        var logged = new List<string>();
        LiteNet2Devices.Log = logged.Add;

        var connected = Connected("127.0.0.7", "S7A", 7);
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { connected });

        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { Disconnected("127.0.0.7", "S7B", 7) });

        var boards = LiteNet2Devices.Boards!;
        Assert.Single(boards);
        Assert.Same(connected, boards[0]);
        Assert.Equal("S7A", boards[0].SerialNumber);
        Assert.Contains(logged, l => l.Contains("Divergência"));
        LiteNet2Devices.Log = null;
    }

    // Enumeração concorrente com a varredura não lança (thread-safety).
    [Fact]
    public async Task Concurrent_enumeration_during_merge_does_not_throw()
    {
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { Disconnected("127.0.0.20", "S20", 20) });

        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));
        var reader = Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
                foreach (var b in LiteNet2Devices.Boards ?? new List<LiteNet2Board>())
                    _ = b.Ip;
        });

        var writer = Task.Run(() =>
        {
            var i = 21;
            while (!cts.IsCancellationRequested)
                LiteNet2Devices.SetBoards(new List<LiteNet2Board>
                {
                    Disconnected($"127.0.0.{i % 200 + 30}", $"S{i++}", i)
                });
        });

        await Task.WhenAll(reader, writer); // não deve lançar
    }
}
