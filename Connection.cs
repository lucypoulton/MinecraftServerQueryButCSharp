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
        Console.WriteLine("Handshaking");
        Stream.WriteVarInt(0);
        Stream.WriteVarInt(ProtocolVersion);
        Stream.WriteString(EndPoint.Address.ToString());
        byte[] port = { (byte)(EndPoint.Port << 8), (byte)EndPoint.Port };
        Stream.Write(port);
        Stream.WriteVarInt((int) state);
        await Stream.Flush();
        await Stream.ReadPacket();
        Stream.ReadVarInt();
        Console.WriteLine(Stream.ReadString());
        Console.WriteLine("Handshaking OK");
    }

    public async Task Status()
    {
        Console.WriteLine("Getting status");
        Stream.WriteVarInt(0);
        await Stream.Flush();
        Console.WriteLine("Reading packet");
        await Stream.ReadPacket();
        Console.WriteLine("Parsing");
        Stream.ReadVarInt();
        Console.WriteLine(Stream.ReadString());
    }

    public void Dispose()
    {
        Socket.Dispose();
        Stream.Dispose();
    }
}