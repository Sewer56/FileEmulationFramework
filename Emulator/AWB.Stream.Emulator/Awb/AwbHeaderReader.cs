using System.Runtime.InteropServices;
using AwbLib.Structs;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.Streams;

namespace AWB.Stream.Emulator.Awb;

/// <summary>
/// Utility class that can be used to read out the AFS header from a given stream.
/// </summary>
public static unsafe class AwbHeaderReader
{
    /// <summary>
    /// Tries to read the header of an AFS file.
    /// </summary>
    /// <returns>False if the stream does not contain an AFS2 file or the stream could not be read to end.</returns>
    public static bool TryReadHeader(System.IO.Stream stream, out byte[]? data)
    {
        if (!stream.TryRead(out Afs2Header header, out _))
        {
            data = null;
            return false;
        }
        
        // Assert this is AFS2
        header.AssertSupportedArchive();

        // Get header size
        var headerSize = header.GetTotalSizeOfHeader();
        
        data = GC.AllocateUninitializedArray<byte>(headerSize);
        MemoryMarshal.Cast<byte, Afs2Header>(data)[0] = header; // Write to start of array.
        return stream.TryRead(data.AsSpanFast(sizeof(Afs2Header), headerSize - sizeof(Afs2Header)), out _);
    }
}