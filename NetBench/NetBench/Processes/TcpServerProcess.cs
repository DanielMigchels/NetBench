using NetBench.Processes.States;
using NetBench.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetBench.Processes;

public class TcpServerProcess(string hostUri) : Process($"server-{hostUri}")
{
    private TcpListener? _listener;
    private readonly TcpServerProcessState _state = new();

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var endpoint = IpUtils.ParseEndpoint(hostUri);
        _listener = new TcpListener(endpoint);
        _listener.Start();

        _state.ServerEndpoint = endpoint?.ToString() ?? "Unknown";
        await StateChannelWriter.WriteAsync(_state, cancellationToken);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        if (client.Client.RemoteEndPoint is not IPEndPoint remoteEndpoint)
        {
            return;
        }

        var remoteEndpointState = new TcpServerRemoteEndpointState()
        {
            RemoteEndpoint = $"{remoteEndpoint.Address}:{remoteEndpoint.Port}",
            Connected = true,
        };
        _state.RemoteEndpoints.Add(remoteEndpointState);
        await StateChannelWriter.WriteAsync(_state, cancellationToken);

        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[65_536];
            var stopwatch = Stopwatch.StartNew();
            long bytesReceived = 0;
            double? previousMbps = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                int read;
                try
                {
                    read = await stream.ReadAsync(buffer, cancellationToken);
                }
                catch (IOException)
                {
                    break;
                }

                if (read == 0)
                {
                    break;
                }

                bytesReceived += read;

                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    var mbps = bytesReceived * 8 / stopwatch.Elapsed.TotalSeconds / 1000000;

                    remoteEndpointState.Throughput = mbps;
                    await StateChannelWriter.WriteAsync(_state, cancellationToken);

                    previousMbps = mbps;
                    bytesReceived = 0;
                    stopwatch.Restart();
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            remoteEndpointState.Connected = false;
            remoteEndpointState.Throughput = 0;
            await StateChannelWriter.WriteAsync(_state, cancellationToken);
            client.Dispose();
        }
    }

    public override ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return base.DisposeAsync();
    }
}
