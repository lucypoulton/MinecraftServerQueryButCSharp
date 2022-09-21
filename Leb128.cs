namespace MinecraftQuery;

/**
 * Helper functions for reading/writing signed LEB-128 encoded integers to/from streams.
 */
public static class Leb128
{
    private const byte ContinueBit = 0b_1000_0000;
    private const byte SegmentBits = 0b_0111_1111;

    /**
     * <summary>
     * Encodes the given integer to LEB-128 and writes it to a stream.
     * </summary>
     */
    public static async void Write(Stream stream, int value)
    {
        var output = new MemoryStream();
        for (var shouldContinue = true; shouldContinue;)
        {
            shouldContinue = value > SegmentBits;
            var nextByte = (byte)(SegmentBits & value);
            if (shouldContinue) nextByte |= ContinueBit;
            output.WriteByte(nextByte);
            value >>>= 7;
        }

        await stream.WriteAsync(output.GetBuffer(), 0, (int)output.Length);
    }
    
    /**
     * <summary>
     * Reads a LEB-128 encoded integer from the stream.
     * </summary>
     * FIXME - make this async somehow
     */
    public static int Read(Stream stream)
    {
        var output = 0;
        var shift = 0;
        for (var shouldContinue = true; shouldContinue;)
        {
            var next = stream.ReadByte();
            shouldContinue = (ContinueBit & (byte)next) == ContinueBit;
            output |= (next & SegmentBits) << shift;
            shift += 7;
        }

        return output;
    }
}