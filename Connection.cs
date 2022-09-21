using System.Net;
using System.Net.Sockets;

namespace MinecraftQuery;

public class Connection : IDisposable
{
    public enum HandshakeState
    {
        Status = 1,
        Login = 2
    }
    
    public IPEndPoint EndPoint { get; }

    private Socket Socket { get; }

    private ProtocolStream Stream { get; }

    public int ProtocolVersion { get; }
    

    public Connection(IPEndPoint endPoint, int protocolVersion = 760)
    {
        ProtocolVersion = protocolVersion;
        EndPoint = endPoint;
        Socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket.Connect(endPoint);
        Stream = new ProtocolStream(Socket);
    }

    public async Task Handshake(HandshakeState state)
    {
        Stream.WriteVarInt(0);
        Stream.WriteVarInt(ProtocolVersion);
        Stream.WriteString(EndPoint.Address.ToString());
        byte[] port = { (byte)(EndPoint.Port << 8), (byte)EndPoint.Port };
        Stream.Write(port);
        Stream.WriteVarInt((int) state);
        await Stream.Flush();
    }

    public async Task<string> Status()
    {
        Stream.WriteVarInt(0);
        await Stream.Flush();
        await Stream.ReadPacket();
        Stream.ReadVarInt();
        return Stream.ReadString();
    }

    public void Dispose()
    {
        Socket.Dispose();
        Stream.Dispose();
    }
}