using NetBench.Outputs;
using NetBench.Processes;

namespace NetBench;

public class ApplicationSession : IAsyncDisposable
{
    private readonly List<IOutput> _outputs = [];
    private readonly List<IProcess> _processes = [];
    private readonly CancellationTokenSource _cts = new();

    public void AddOutput(IOutput output) => _outputs.Add(output);
    public void AddProcess(IProcess process) => _processes.Add(process);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        var token = linkedCts.Token;

        var eventTasks = _processes.Select(node => Task.Run(() => StateUpdatedAsync(node))).ToList();
        var processTasks = _processes.Select(node => Task.Run(() => node.StartAsync(token), CancellationToken.None)).ToList();

        try
        {
            await Task.WhenAll(processTasks);
        }
        catch (OperationCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException)) {  }
    }

    private async Task StateUpdatedAsync(IProcess process)
    {
        try
        {
            await foreach (var state in process.StateChannelReader.ReadAllAsync(CancellationToken.None))
            {
                foreach (var reporter in _outputs)
                {
                    reporter.ReportState(state);
                }
            }
        }
        catch (OperationCanceledException) {  }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();

        foreach (var process in _processes)
        {
            await process.DisposeAsync();
        }
        
        foreach (var output in _outputs)
        {
            output.Dispose();
        }
        
        _cts.Dispose();

        GC.SuppressFinalize(this);
    }
}