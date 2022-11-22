using Reloaded.Memory.Streams;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace AWB.Stream.Emulator.Awb.Utilities;

/// <summary>
/// Utility class for writing numbers.
/// </summary>
public static class ValueWriters
{
    /// <summary>
    /// Writes the number from a long, using the specified bit count [multiple of byte].
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="size">Size of the data in bits. Accepts 8/16/32/64.</param>
    public static void WriteNumber(this MemoryStream stream, long value, int size)
    {
        switch (size)
        {
            case 1:
                stream.Write<byte>((byte)value);
                break;
            case 2:                
                stream.Write<short>((short)value);
                break;
            case 4:
                stream.Write<int>((int)value);
                break;
            case 8:
                stream.Write<long>(value);
                break;
            default:
                ThrowHelpers.ThrowBadFieldSizeException();
                break;
        }
    }
}