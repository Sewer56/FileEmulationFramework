using AWB.Stream.Emulator.Awb.Structs;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace AWB.Stream.Emulator.Awb.Utilities;

/// <summary>
/// Checks if the file is an AWB file.
/// </summary>
public static class AwbChecker
{
    /// <summary>
    /// Checks if a file with a given handle is an AFS file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsAwbFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;

        try
        {
            return fileStream.Read<int>() == Afs2Header.ExpectedMagic; // 'AFS2'
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}