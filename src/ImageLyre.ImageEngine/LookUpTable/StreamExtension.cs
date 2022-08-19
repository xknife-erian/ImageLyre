using System;
using System.IO;

namespace ImageLyre.ImageEngine.LookUpTable;

public static class StreamExtension
{
    public static bool ReadBoolean(this Stream stream)
    {
        var ch = stream.ReadByte();
        if (ch < 0)
            throw new EOFException();
        return ch != 0;
    }

    public static byte ReadByte(this Stream stream)
    {
        var ch = stream.ReadByte();
        if (ch < 0)
            throw new EOFException();
        return (byte) ch;
    }

    public static int ReadUnsignedByte(this Stream stream)
    {
        var ch = stream.ReadByte();
        if (ch < 0)
            throw new EOFException();
        return ch;
    }

    public static short ReadShort(this Stream stream)
    {
        var ch1 = stream.ReadByte();
        var ch2 = stream.ReadByte();
        if ((ch1 | ch2) < 0)
            throw new EOFException();
        return (short) ((ch1 << 8) + (ch2 << 0));
    }

    public static int ReadUnsignedShort(this Stream stream)
    {
        var ch1 = stream.ReadByte();
        var ch2 = stream.ReadByte();
        if ((ch1 | ch2) < 0)
            throw new EOFException();
        return (ch1 << 8) + (ch2 << 0);
    }

    public static char ReadChar(this Stream stream)
    {
        var ch1 = stream.ReadByte();
        var ch2 = stream.ReadByte();
        if ((ch1 | ch2) < 0)
            throw new EOFException();
        return (char) ((ch1 << 8) + (ch2 << 0));
    }

    public static int ReadInt(this Stream stream)
    {
        var ch1 = stream.ReadByte();
        var ch2 = stream.ReadByte();
        var ch3 = stream.ReadByte();
        var ch4 = stream.ReadByte();
        if ((ch1 | ch2 | ch3 | ch4) < 0)
            throw new EOFException();
        return (ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0);
    }

    public static long ReadLong(this Stream stream)
    {
        byte[] readBuffer = new byte[8];
        var read = stream.Read(readBuffer, 0, 8);
        return ((long) readBuffer[0] << 56) +
               ((long) (readBuffer[1] & 255) << 48) +
               ((long) (readBuffer[2] & 255) << 40) +
               ((long) (readBuffer[3] & 255) << 32) +
               ((long) (readBuffer[4] & 255) << 24) +
               ((readBuffer[5] & 255) << 16) +
               ((readBuffer[6] & 255) << 8) +
               ((readBuffer[7] & 255) << 0);
    }

    public static float ReadFloat(this Stream stream)
    {
        return IntBitsToFloat(ReadInt(stream));
    }

    public static double ReadDouble(this Stream stream)
    {
        return LongBitsToDouble(ReadLong(stream));
    }

    private static float IntBitsToFloat(int v)
    {
        var buf = BitConverter.GetBytes(v);
        return BitConverter.ToSingle(buf, 0);
    }

    private static long LongBitsToDouble(long v)
    {
        var buf = BitConverter.GetBytes(v);
        return BitConverter.ToInt64(buf, 0);
    }
}