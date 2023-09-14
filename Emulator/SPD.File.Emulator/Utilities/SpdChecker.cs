using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.Utilities;
using SPD.File.Emulator.Spd;

namespace SPD.File.Emulator.Utilities;

public static class SpdChecker
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
            //read spd magic
            byte[] magic = new byte[4];
            fileStream.ReadAtLeast(magic, 4);

            //return false if the magic is not 'SPR0', otherwise return true
            return !(magic[0] != 'S' || magic[1] != 'P' || magic[2] != 'R' || magic[3] != '0');
        }
        finally
        {
            fileStream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}
