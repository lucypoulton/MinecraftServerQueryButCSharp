using System.Net.Sockets;
using System.Text;
using MinecraftQuery.Util;

namespace MinecraftQuery;

public class ProtocolStream : IDisposable
{
    private Socket Socket { get; }

    private Stream SocketStream { get; }

    // we're only expecting to write handshake/meta packets so this should be plenty
    private MemoryStream OutBuffer { get; set; } = new(256);
    private MemoryStream? InBuffer { get; set; }

    public ProtocolStream(Socket socket)
    {
        Socket = socket;
        SocketStream = new NetworkStream(socket);
        SocketStream.ReadTimeout = 2000;
    }

    public async Task Flush()
    {
        var stream = new NetworkStream(Socket);
        Leb128.Write(stream, (int)OutBuffer.Length);
        await stream.WriteAsync(OutBuffer.GetBuffer(), 0, (int) OutBuffer.Length + 1);
        await OutBuffer.DisposeAsync();
        OutBuffer = new(256);
    }

    public void Write(byte[] bytes)
    {
        OutBuffer.Write(bytes);
    }

    public void WriteVarInt(int value)
    {
        Leb128.Write(OutBuffer, value);
    }

    public int ReadVarInt()
    {
        if (InBuffer == null) throw new NullReferenceException("A packet has not been read");
        return Leb128.Read(InBuffer);
    }

    public void WriteString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteVarInt(bytes.Length);
        OutBuffer.Write(bytes);
    }

    public string ReadString()
    {
        if (InBuffer == null) throw new NullReferenceException("A packet has not been read");
        var length = Leb128.Read(InBuffer);
        return Encoding.UTF8.GetString(InBuffer.GetBuffer(), (int)InBuffer.Position, length);
    }

    private async Task ReadPacketInternal()
    {
        var length = Leb128.Read(SocketStream);
        var buffer = new byte[length];
        var read = 0;
        do
        {
            read += await SocketStream.ReadAsync(buffer, read, length - read);
        } while (read < length);
        InBuffer = new MemoryStream(buffer, 0, buffer.Length, true, true);
    }

    public async Task ReadPacket()
    {
        await ReadPacketInternal().WaitAsync(TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        Socket.Dispose();
        SocketStream.Dispose();
        OutBuffer.Dispose();
        InBuffer?.Dispose();
    }
}