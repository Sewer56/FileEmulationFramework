using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BF.File.Emulator.Utilities;

/// <summary>
/// Verifies FLW0 header of a file.
/// </summary>
public static class BfChecker
{
    /// <summary>
    /// Checks if a file with a given handle is a BF file or an empty file (0 length).
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsBfFileOrEmpty(IntPtr handle, out bool isEmpty)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        isEmpty = fileStream.Length == 0;
        if(isEmpty)
        {
            fileStream.Dispose();
            return true;
        }
        var pos = fileStream.Position;

        try
        {
            return HasBfHeader(fileStream);
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    private static bool HasBfHeader(FileStream stream)
    {
        if (stream.Length < 12)
            return false;
        var pos = stream.Position;
        try
        {
            // read header
            byte[] header = new byte[12];
            stream.ReadAtLeast(header, 12);

            // Check if magic is correct
            if (header[8] != 'F' || header[9] != 'L' || header[10] != 'W' || header[11] != '0')
                return false;

            // Header magic is right, it's probably a valid bf
            return true;
        }
        finally
        {
            stream.Seek(pos, SeekOrigin.Begin);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this System.IO.Stream stream) where T : unmanaged
    {
        Span<T> stackSpace = stackalloc T[1];
        stream.TryRead(MemoryMarshal.Cast<T, byte>(stackSpace), out _);
        return stackSpace[0];
    }
}
