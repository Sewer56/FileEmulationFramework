using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.Utilities;
using SPD.File.Emulator.Spd;

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
            //read spd magic
            byte[] spdMagic = new byte[4];
            fileStream.ReadAtLeast(spdMagic, 4);

            byte[] sprMagic = new byte[4];
            fileStream.Seek(8, SeekOrigin.Begin);
            fileStream.ReadAtLeast(sprMagic, 4);
            //return false if the magic is not 'SPR0', otherwise return true
            return !(spdMagic[0] != 'S' || spdMagic[1] != 'P' || spdMagic[2] != 'R' || spdMagic[3] != '0') 
                || !(sprMagic[0] != 'S' || sprMagic[1] != 'P' || sprMagic[2] != 'R' || sprMagic[3] != '0');
        }
        finally
        {
            fileStream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}
