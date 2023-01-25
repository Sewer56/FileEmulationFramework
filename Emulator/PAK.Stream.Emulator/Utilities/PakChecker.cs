using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.Utilities;
using System.Net.NetworkInformation;
using PAK.Stream.Emulator.Pak;

namespace PAK.Stream.Emulator.Utilities;

/// <summary>
/// Verifies PAK header of a file.
/// </summary>
public static class PakChecker
{
    /// <summary>
    /// Checks if a file with a given handle is an PAK file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsPakFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;
        try
        {
            return IsPakFileHelper(fileStream);
        }
        finally
        {
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    public static bool IsPakFile(byte[] bytes)
    {
        var fileStream = new MemoryStream(bytes);

        return IsPakFileHelper(fileStream);
    }

    private static bool IsPakFileHelper(System.IO.Stream stream)
    {
        try
        {
            return !(PakBuilder.DetectVersion(stream) == FormatVersion.Unknown);
        }
        finally
        {
            stream.Dispose();
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