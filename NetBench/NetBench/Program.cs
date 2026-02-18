using NetBench;
using NetBench.Outputs;
using NetBench.Processes;

var applicationArguments = ApplicationArgumentParser.Parse(args);

if (applicationArguments.ServerEndpoints.Count == 0 && applicationArguments.ClientEndpoints.Count == 0)
{
    Console.WriteLine("Welcome to NetBench! (https://github.com/DanielMigchels/NetBench)");
    Console.WriteLine("Add a server or client to start benchmarking.");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  netbench -s <host:port>      Start a TCP server (Add as many as you need)");
    Console.WriteLine("  netbench -c <host:port>      Start a TCP client (Add as many as you need)");
    Console.WriteLine("  netbench -o <file>           Write network report to file");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  netbench -s 0.0.0.0:5000");
    Console.WriteLine("  netbench -c 192.168.1.100:5000");
    Console.WriteLine("  netbench -c 192.168.1.100:5000 -o output.txt");
    return;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await using var applicationSession = new ApplicationSession();

applicationSession.AddOutput(new ConsoleOutput());

if (!string.IsNullOrWhiteSpace(applicationArguments.OutputFile))
{
    applicationSession.AddOutput(new FileReportOutput(applicationArguments.OutputFile));
}

foreach (var serverEndpoint in applicationArguments.ServerEndpoints)
{
    applicationSession.AddProcess(new TcpServerProcess(serverEndpoint));
}

foreach (var clientEndpoint in applicationArguments.ClientEndpoints)
{
    applicationSession.AddProcess(new TcpClientProcess(clientEndpoint));
}

await applicationSession.RunAsync(cts.Token);