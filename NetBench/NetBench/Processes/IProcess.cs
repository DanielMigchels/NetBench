using NetBench.Processes.States;
using System.Threading.Channels;

namespace NetBench.Processes;

public interface IProcess : IAsyncDisposable
{
    public ChannelReader<ProcessState> StateChannelReader { get; }
    public ChannelWriter<ProcessState> StateChannelWriter { get; }
    Task StartAsync(CancellationToken cancellationToken);
}