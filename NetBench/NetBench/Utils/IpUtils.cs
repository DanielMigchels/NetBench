using System.Net;

namespace NetBench.Utils;

public class IpUtils
{
    public static IPEndPoint ParseEndpoint(string hostUri)
    {
        var lastColon = hostUri.LastIndexOf(':');
        if (lastColon < 0)
        {
            throw new FormatException($"Invalid endpoint format: '{hostUri}'. Expected 'host:port'.");
        }

        var host = hostUri[..lastColon];
        var port = int.Parse(hostUri[(lastColon + 1)..]);
        var address = IPAddress.Parse(host);

        return new IPEndPoint(address, port);
    }
}
