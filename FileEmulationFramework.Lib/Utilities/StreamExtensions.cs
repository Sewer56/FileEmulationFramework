using System.Runtime.CompilerServices;

namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Provides extensions to use with streams.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Reads an unmanaged, generic type from the stream.
    /// </summary>
    /// <param name="stream">The stream to read the value from.</param>
    /// <param name="value">The value to return.</param>
    /// <param name="numBytesRead">Number of bytes actually read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryRead<T>(this Stream stream, out T value, out int numBytesRead) where T : unmanaged
    {
        value = default;
        var valueSpan = new Span<byte>(Unsafe.AsPointer(ref value), sizeof(T));
        return TryRead(stream, valueSpan, out numBytesRead);
    }

    /// <summary>
    /// Tries to read a given number of bytes from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the value from.</param>
    /// <param name="result">The buffer to receive the bytes.</param>
    /// <param name="numBytesRead">Number of bytes actually read.</param>
    /// <returns>True if all bytes have been read, else false.</returns>
    /// <remarks>This function is equivalent to .NET 7's ReadExactly, except that it does not throw, instead returns false.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRead(this Stream stream, Span<byte> result, out int numBytesRead)
    {
        numBytesRead = 0;
        int numBytesToRead = result.Length;

        while (numBytesToRead > 0)
        {
            int bytesRead = stream.Read(result.Slice(numBytesRead, numBytesToRead));
            if (bytesRead <= 0)
                return false;

            numBytesRead += bytesRead;
            numBytesToRead -= bytesRead;
        }

        return true;
    }
}