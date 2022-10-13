using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace ONE.Heroes.Stream.Emulator.Utilities;

/// <summary>
/// Quick check to verify header of Heroes' ONE file.
/// This isn't a comprehensive check, just a trivial one to 100% be sure.
/// </summary>
public static class OneChecker
{
    /// <summary>
    /// Checks if a file with a given handle is an ONE file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsOneFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var originalPos = fileStream.Position;

        try
        {
            fileStream.Seek(4, SeekOrigin.Begin);
            fileStream.TryRead(out int lengthOfRestOfFile, out _);
            return fileStream.Length == (lengthOfRestOfFile + 12); // 12 is size of RW stream chunk header
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, originalPos, IntPtr.Zero, 0);
        }
    }
}