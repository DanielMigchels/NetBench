namespace NetBench;

public class ApplicationArguments
{
    public List<string> ServerEndpoints { get; init; } = [];
    public List<string> ClientEndpoints { get; init; } = [];
    public string? OutputFile { get; init; }
    public double? ThroughputLimitMbps { get; init; }
}
