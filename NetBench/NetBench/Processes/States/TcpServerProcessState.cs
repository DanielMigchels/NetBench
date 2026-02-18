namespace NetBench.Processes.States;

public class TcpServerProcessState : ProcessState
{
    public string ServerEndpoint { get; set; } = string.Empty;
    public List<TcpServerRemoteEndpointState> RemoteEndpoints { get; set; } = [];
}
