using MinecraftQuery;

var result = await Resolver.Resolve("node.lucypoulton.net:25566");
await Console.Out.WriteLineAsync(result?.Address + " " + result?.Port);

if (result == null) return;

using var con = new Connection(result);
await con.Handshake(Connection.HandshakeState.Status);
Console.WriteLine(await con.Status());