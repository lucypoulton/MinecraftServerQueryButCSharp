using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace MinecraftQuery;

public class Connection : IDisposable
{
    public enum HandshakeState
    {
        Status = 1,
        Login = 2
    }

    public IPEndPoint EndPoint { get; }
    
    public string Hostname { get; }

    private Socket Socket { get; }

    private ProtocolStream Stream { get; }

    public int ProtocolVersion { get; }

    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Connection(IPEndPoint endPoint, string hostname, int protocolVersion = 760)
    {
        ProtocolVersion = protocolVersion;
        EndPoint = endPoint;
        Hostname = hostname;
        Socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket.Connect(endPoint);
        Stream = new ProtocolStream(Socket);
    }

    public async Task Handshake(HandshakeState state)
    {
        Stream.WriteVarInt(0);
        Stream.WriteVarInt(ProtocolVersion);
        Stream.WriteString(Hostname);
        byte[] port = { (byte)(EndPoint.Port << 8), (byte)EndPoint.Port };
        Stream.Write(port);
        Stream.WriteVarInt((int)state);
        await Stream.Flush();
    }

    public async Task<ServerStatus> Status()
    {
        Stream.WriteVarInt(0);
        await Stream.Flush();
        await Stream.ReadPacket();
        Stream.ReadVarInt();
        return JsonSerializer.Deserialize<ServerStatus>(Stream.ReadString(), options);
    }

    public void Dispose()
    {
        Socket.Dispose();
        Stream.Dispose();
    }
}