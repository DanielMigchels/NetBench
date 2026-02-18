using NetBench.Processes.States;
using System.Text;

namespace NetBench.Outputs;

public class ConsoleOutput : IOutput
{
    private readonly Lock _lock = new();
    private readonly string _header = "Welcome to NetBench! (https://github.com/DanielMigchels/NetBench)";

    private readonly List<ProcessState> ProcessStates = [];

    private int _previousLineCount;

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

            Redraw();
        }
    }

    private void Redraw()
    {
        var lines = new List<string> { _header, "" };

        foreach (var processState in ProcessStates)
        {
            if (processState is TcpServerProcessState serverState)
            {
                lines.Add($"TCP server started on {serverState.ServerEndpoint}");
                lines.Add("Connected clients:");
                foreach (var remoteEndpoints in serverState.RemoteEndpoints)
                {
                    lines.Add($"  - {remoteEndpoints.RemoteEndpoint} [{(remoteEndpoints.Connected ? "Connected" : "Disconnected")}, {remoteEndpoints.Throughput:F0}mbps]");
                }
                lines.Add(string.Empty);
            }
        }

        foreach (var processState in ProcessStates)
        {
            if (processState is TcpClientProcessState clientState)
            {
                lines.Add($"TCP client started to {clientState.Endpoint}");
                lines.Add("Connection info:");
                lines.Add($"  - {clientState.Endpoint} [{(clientState.Connected ? "Connected" : "Disconnected")}, {clientState.Throughput:F0}mbps]");
                lines.Add(string.Empty);
            }
        }

        var width = Math.Max(Console.BufferWidth, 80);
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine(line.Length >= width ? line[..width] : line.PadRight(width));
        }

        for (var i = lines.Count; i < _previousLineCount; i++)
        {
            sb.AppendLine(new string(' ', width));
        }

        _previousLineCount = lines.Count;

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);
        Console.Write(sb);
        Console.CursorVisible = true;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}