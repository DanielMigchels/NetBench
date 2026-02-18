using NetBench.Processes.States;

namespace NetBench.Outputs;

public interface IOutput : IDisposable
{
    void ReportState(ProcessState state);
}