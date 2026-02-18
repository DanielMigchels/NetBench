using NetBench.Processes.States;
using System.Threading.Channels;

namespace NetBench.Processes;

public abstract class Process : IProcess
{
    private readonly Channel<ProcessState> _stateChannel;
    public ChannelReader<ProcessState> StateChannelReader => _stateChannel.Reader;
    public ChannelWriter<ProcessState> StateChannelWriter => _stateChannel.Writer;

    public string Id { get; }

    public Process(string id)
    {
        Id = id;

        _stateChannel = Channel.CreateUnbounded<ProcessState>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public abstract Task StartAsync(CancellationToken cancellationToken);

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}