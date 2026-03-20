using NetBench.Processes.States;

namespace NetBench.Outputs;

public class FileReportOutput : IOutput
{
    private readonly StreamWriter _writer;
    private readonly Lock _lock = new();
    private readonly List<ProcessState> ProcessStates = [];
    private readonly string _header = "Welcome to NetBench! (https://github.com/DanielMigchels/NetBench)";

    public FileReportOutput(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileExists = File.Exists(filePath);

        _writer = new StreamWriter(filePath, append: true)
        {
            AutoFlush = true
        };

        if (!fileExists)
        {
            _writer.WriteLine(_header);
        }

        System.Timers.Timer timer = new System.Timers.Timer(1000);
        timer.Elapsed += (sender, e) => WriteReport();
        timer.Start();
    }

    public void ReportState(ProcessState state)
    {
        lock (_lock)
        {
            var processState = ProcessStates.FirstOrDefault(x => x.Id == state.Id);
            if (processState != null)
            {
                processState = state;
            }
            else
            {
                ProcessStates.Add(state);
            }
        }
    }

    private void WriteReport()
    {
        var line = string.Empty;
        line += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t";

        foreach (var processState in ProcessStates)
        {
            if (processState is TcpServerProcessState serverState)
            {
                line += "TCPServer\t";
                line += $"{serverState.ServerEndpoint}\t";
                line += $"Connected clients:\t";
                line += $"{serverState.RemoteEndpoints.Count}\t";
            }
        }

        foreach (var processState in ProcessStates)
        {
            if (processState is TcpClientProcessState clientState)
            {
                line += "TCPClient\t";
                line += $"{clientState.Endpoint}\t";
                line += $"{(clientState.Connected ? "Connected" : "Disconnected")}\t";
                line += $"{clientState.Throughput:F0}mbps\t";
            }
        }

        _writer.WriteLine(line);
    }

    public void Dispose()
    {
        _writer.Dispose();
        GC.SuppressFinalize(this);
    }
}
