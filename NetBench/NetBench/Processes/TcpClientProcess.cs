using NetBench.Processes.States;
using NetBench.Utils;
using System.Diagnostics;
using System.Net.Sockets;

namespace NetBench.Processes;

public class TcpClientProcess(string serverUri) : Process($"client-{serverUri}")
{
    private readonly TcpClientProcessState _state = new();

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var endpoint = IpUtils.ParseEndpoint(serverUri);
        _state.Endpoint = endpoint.ToString();
        await StateChannelWriter.WriteAsync(_state, cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            using var client = new TcpClient();

            try
            {
                await client.ConnectAsync(endpoint, cancellationToken);
            }
            catch (SocketException) when (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(500, cancellationToken);
                continue;
            }

            _state.Connected = true;
            await StateChannelWriter.WriteAsync(_state, cancellationToken);

            var isGracefulShutdown = await RunTransferLoopAsync(client, cancellationToken);

            _state.Connected = false;
            _state.Throughput = 0;
            await StateChannelWriter.WriteAsync(_state, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            if (!isGracefulShutdown)
            {
                await Task.Delay(500, cancellationToken);
                continue;
            }
            else
            {
                break;
            }
        }
    }

    private async Task<bool> RunTransferLoopAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = client.GetStream();

            var buffer = new byte[65536];
            Random.Shared.NextBytes(buffer);

            var stopwatch = Stopwatch.StartNew();
            long bytesSent = 0;
            double? previousMbps = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await stream.WriteAsync(buffer, cancellationToken);
                    bytesSent += buffer.Length;
                }
                catch (IOException)
                {
                    return false;
                }

                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    var mbps = bytesSent * 8.0 / stopwatch.Elapsed.TotalSeconds / 1000000;

                    _state.Throughput = mbps;
                    await StateChannelWriter.WriteAsync(_state, cancellationToken);

                    previousMbps = mbps;
                    bytesSent = 0;
                    stopwatch.Restart();
                }
            }
        }
        catch (OperationCanceledException) { }

        return true;
    }

    public override ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return base.DisposeAsync();
    }
}
