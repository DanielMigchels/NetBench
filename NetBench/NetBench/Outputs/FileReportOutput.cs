using NetBench.Processes.States;

namespace NetBench.Outputs;

public class FileReportOutput : IOutput
{
    private readonly StreamWriter _writer;
    private readonly Lock _lock = new();

    public FileReportOutput(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(filePath, append: true)
        {
            AutoFlush = true
        };
    }

    public void Dispose()
    {
        _writer.Dispose();
        GC.SuppressFinalize(this);
    }

    public void ReportState(ProcessState state)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Work in progress.";

        lock (_lock)
        {
            _writer.WriteLine(line);
        }
    }
}
