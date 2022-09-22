using System.Net;
using System.Text.RegularExpressions;
using DnsClient;
using DnsClient.Protocol;

namespace MinecraftQuery.Util;

public static class Resolver
{
    private static readonly Regex IpRegex = new(
        @"((?<ip>(\d{1,3}\.){3}\d{1,3})|(?<domain>[a-zA-Z\-\d.]+)):?(?<port>\d{1,5})");

    private static readonly LookupClient Client = new();

    public static async Task<KeyValuePair<string, IPEndPoint>?> Resolve(string name)
    {
        var match = IpRegex.Match(name);
        ushort port = 25565;
        if (match.Success)
        {
            if (match.Groups["ip"].Success)
                return new(
                    match.Groups["ip"].Value,
                    new IPEndPoint(
                        IPAddress.Parse(match.Groups["ip"].Value),
                        ushort.Parse(match.Groups["port"].Value)
                    )
                );
            name = match.Groups["domain"].Value;
            port = ushort.Parse(match.Groups["port"].Value);
        }
        else
        {
            var srvResult = await Client.QueryAsync($"_minecraft._tcp.{name}", QueryType.SRV);
            if (srvResult is { HasError: false } && srvResult.Answers[0] is SrvRecord srv)
            {
                port = srv.Port;
                name = srv.Target;
            }
        }

        var aResult = await Client.QueryAsync(name, QueryType.A);
        if (aResult is { HasError: false })
        {
            var a = (ARecord)aResult.Answers.First(record => record is ARecord);
            if (a != null) return new(name, new IPEndPoint(a.Address, port));
        }

        return null;
    }
}