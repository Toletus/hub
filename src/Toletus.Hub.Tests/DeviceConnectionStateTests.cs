using System.Net;
using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Models;
using Toletus.Hub.Services.NotificationsServices.Base;
using Toletus.LiteNet2;
using Xunit;

namespace Toletus.Hub.Tests;

/// <summary>
/// "device is not in connected" com a catraca operando: o guard lia um snapshot do cache
/// Devices (capturado no Connect) em vez do board vivo. Para LiteNet2 a fonte de verdade
/// passa a ser o board.
/// </summary>
public class DeviceConnectionStateTests : IDisposable
{
    private readonly LoopbackServer _server = new();

    private sealed class Probe : NotificationBaseService
    {
        public static Device Refresh(Device device) => RefreshDeviceConnectionState(device);
    }

    public DeviceConnectionStateTests() => LiteNet2Devices.Clear();

    public void Dispose()
    {
        LiteNet2Devices.Clear();
        _server.Dispose();
    }

    [Fact]
    public void LiteNet2_connection_state_comes_from_live_board_not_stale_cache()
    {
        const string ip = "127.0.0.30";
        var board = new LiteNet2Board(IPAddress.Parse(ip), "S30", 30) { ConnectPort = _server.Port };
        board.Connect();
        Assert.True(board.Connected);

        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { board });

        // Cache Devices vazio (é exatamente o cenário do "already connected" com Data nulo).
        var device = new Device { Ip = ip, Type = DeviceType.LiteNet2, SerialNumber = "S30" };

        var refreshed = Probe.Refresh(device);

        Assert.True(refreshed.Connected); // antes vinha false e o comando era recusado
    }

    [Fact]
    public void LiteNet2_disconnected_board_reports_disconnected()
    {
        const string ip = "127.0.0.31";
        var board = new LiteNet2Board(IPAddress.Parse(ip), "S31", 31); // nunca conectado
        LiteNet2Devices.SetBoards(new List<LiteNet2Board> { board });

        var device = new Device { Ip = ip, Type = DeviceType.LiteNet2, SerialNumber = "S31" };

        Assert.False(Probe.Refresh(device).Connected);
    }
}
