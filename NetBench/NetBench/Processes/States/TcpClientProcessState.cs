namespace NetBench.Processes.States;

public class TcpClientProcessState : ProcessState
{
    public string Endpoint { get; set; } = string.Empty;
    public double Throughput { get; internal set; } = 0;
    public bool Connected { get; set; } = false;
}
