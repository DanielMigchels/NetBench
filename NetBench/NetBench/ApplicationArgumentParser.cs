namespace NetBench;

public class ApplicationArgumentParser
{
    public static ApplicationArguments Parse(string[] args)
    {
        var servers = new List<string>();
        var clients = new List<string>();
        string? output = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (i + 1 > args.Length)
            {
                break;
            }

            switch (args[i])
            {
                case "-s":
                    servers.Add(args[i + 1]);
                    break;
                case "-c":
                    clients.Add(args[i + 1]);
                    break;
                case "-o":
                    output = args[i + 1];
                    break;
            }

            i++;
        }

        return new ApplicationArguments
        {
            ServerEndpoints = servers,
            ClientEndpoints = clients,
            OutputFile = output
        };
    }
}