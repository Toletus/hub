using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Toletus.Hub.Tests;

/// <summary>Servidor TCP loopback mínimo: aceita e segura as conexões para que boards reais reportem Connected.</summary>
public sealed class LoopbackServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentBag<Socket> _sockets = new();

    public LoopbackServer()
    {
        // Any: aceita conexões a qualquer 127.0.0.x, permitindo boards com IPs distintos no mesmo host.
        _listener = new TcpListener(IPAddress.Any, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _ = AcceptLoopAsync(_cts.Token);
    }

    public int Port { get; }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _sockets.Add(await _listener.AcceptSocketAsync(ct));
            }
            catch
            {
                return;
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _listener.Stop(); } catch { /* ignore */ }
        foreach (var s in _sockets)
        {
            try { s.Close(); } catch { /* ignore */ }
        }
        _cts.Dispose();
    }
}
