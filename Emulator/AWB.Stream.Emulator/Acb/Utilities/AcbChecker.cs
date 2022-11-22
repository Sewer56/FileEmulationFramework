using System.Buffers.Binary;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace AWB.Stream.Emulator.Acb.Utilities;

/// <summary>
/// Checks whether file is an ACB file.
/// </summary>
public class AcbChecker
{
    /// <summary>
    /// Checks if a file with a given handle is an AFS file.
    /// </summary>
    /// <param name="handle">The file handle to use.</param>
    public static bool IsAcbFile(IntPtr handle)
    {
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;

        try
        {
            var fileLength = fileStream.Length;
            var magic = fileStream.Read<int>();
            
            // Test for ACB
            if (magic == 0x46545540) // '@UTF'
            {
                var acbLength = BinaryPrimitives.ReverseEndianness(fileStream.Read<int>());
                return acbLength == fileLength - 8;
            }
            else if (magic == 0x584442) // 'BDX ' (Bayonetta)
            {
                // Read a bit of the file to make sure.
                if (fileStream.Length < 48)
                    return false;
                
                fileStream.Position = 44;
                var datPtr = fileStream.Read<int>();
                if (fileStream.Length < datPtr)
                    return false;

                fileStream.Position = datPtr;
                return fileStream.Read<int>() == 0x544144;
            }

            return false;
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}