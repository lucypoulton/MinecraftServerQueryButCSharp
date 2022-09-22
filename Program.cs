using MinecraftQuery;
using MinecraftQuery.Util;

if (args.Length == 0)
{
    Console.WriteLine("Usage: mcquery <hostname>");
    Environment.Exit(1);
    return;
}

var endpoint = await Resolver.Resolve(args[0]);

if (endpoint == null)
{
    Console.WriteLine("Failed to resolve hostname");
    return;
}

var connection = new Connection(endpoint.Value.Value, endpoint.Value.Key);

await connection.Handshake(Connection.HandshakeState.Status);
var status = await connection.Status();
Console.WriteLine($@"{endpoint.Value.Key} ({endpoint.Value.Value})
{status.Version.Name} ({status.Version.Protocol})
{status.Players.Online} / {status.Players.Max} players

{ComponentUtil.ToPlainString(status.Description)}");