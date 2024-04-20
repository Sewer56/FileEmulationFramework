using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace SPD.File.Emulator.Utilities;

public static class SpriteChecker
{
    /// <summary>
    /// Checks if a file with a given handle is an SPD file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsSpdFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;

        try
        {
            //read spd magic at offset 0x0
            var spdMagic = fileStream.Read<int>();

            //read magic at 0x8 for sprs
            fileStream.Seek(8, SeekOrigin.Begin);
            var sprMagic = fileStream.Read<int>();

            //return false if the magic is not 'SPR0', otherwise return true
            return spdMagic == 810700883 || sprMagic == 810700883;
        }
        finally
        {
            fileStream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}
