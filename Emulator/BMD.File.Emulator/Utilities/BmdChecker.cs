using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BMD.File.Emulator.Utilities;

public static class BmdChecker
{
    /// <summary>
    /// Checks if a file with a given handle is a BMD file or an empty file (0 length).
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsBmdFileOrEmpty(IntPtr handle, out bool isEmpty)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        isEmpty = fileStream.Length == 0;
        if (isEmpty)
        {
            fileStream.Dispose();
            return true;
        }
        var pos = fileStream.Position;

        try
        {
            return HasBmdHeader(fileStream);
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    private static bool HasBmdHeader(FileStream stream)
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
            if (header[8] == 'M' && header[9] == 'S' && header[10] == 'G' && header[11] == '1')
                return true;

            // order of these bytes may be reversed, check again with reverse order
            if (header[11] == 'M' && header[10] == 'S' && header[9] == 'G' && header[8] == '1')
                return true;

            // Header magic is wrong, probably not valid bmd
            return false;
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
