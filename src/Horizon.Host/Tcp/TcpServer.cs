using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
//namespace Horizon.Host.Tcp
//{
//    public class TcpServer
//    {
//    }
//}
public sealed class TcpServer : BackgroundService
{
    private readonly TcpListener _listener = new(IPAddress.Any, 5055);
    private readonly ConcurrentDictionary<TcpClient, NetworkStream> _clients = new();

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _listener.Start();
        while (!ct.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(ct);
            var stream = client.GetStream();
            _clients[client] = stream;

            _ = Task.Run(async () =>
            {
                while (client.Connected && !ct.IsCancellationRequested)
                    await Task.Delay(1000, ct);
                _clients.TryRemove(client, out _);
                client.Close();
            }, ct);
        }
    }

    public void Broadcast(string line)
    {
        var bytes = Encoding.UTF8.GetBytes(line);
        foreach (var s in _clients.Values)
            if (s.CanWrite) _ = s.WriteAsync(bytes, 0, bytes.Length);
    }

    public override Task StopAsync(CancellationToken ct)
    {
        _listener.Stop();
        foreach (var kv in _clients) { kv.Value.Dispose(); kv.Key.Close(); }
        _clients.Clear();
        return base.StopAsync(ct);
    }
}
