using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.Utilities;
using PAK.Stream.Emulator.Pak;

namespace PAK.Stream.Emulator.Utilities;

/// <summary>
/// Verifies PAK header of a file.
/// </summary>
public static class PakChecker
{
    /// <summary>
    /// Checks if a file with a given handle is a PAK file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsPakFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;

        try
        {
            return !(PakBuilder.DetectVersion(fileStream) == FormatVersion.Unknown);
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
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