namespace NetBench.Processes.States;

public class TcpServerRemoteEndpointState
{
    public string RemoteEndpoint { get; set; } = string.Empty;
    public double Throughput { get; internal set; } = 0;
    public bool Connected { get; set; } = false;
}
