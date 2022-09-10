using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Extensions for dealing with memory.
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// Creates a span for the given byte array without bounds checks.
    /// </summary>
    /// <param name="data">The data to create the span for.</param>
    /// <param name="offset">Offset in the data.</param>
    /// <param name="count">Number of bytes in the span.</param>
    public static Span<byte> AsSpanFast(this byte[] data, int offset, int count)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(data), offset), count);
    }

    /// <summary>
    /// Creates a span for the given byte array without bounds checks.
    /// </summary>
    /// <param name="data">The data to create the span for.</param>
    /// <param name="count">Size of span.</param>
    public static unsafe Span<byte> ToSpanFast(byte* data, int count)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(data), count);
    }
}