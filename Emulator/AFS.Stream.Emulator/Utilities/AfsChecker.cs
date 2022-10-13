using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.Utilities;

namespace AFS.Stream.Emulator.Utilities;

/// <summary>
/// Verifies AFS header of a file.
/// </summary>
public static class AfsChecker
{
    /// <summary>
    /// Checks if a file with a given handle is an AFS file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsAfsFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;

        try
        {
            return Read<int>(fileStream) == 0x534641; // 'AFS'
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