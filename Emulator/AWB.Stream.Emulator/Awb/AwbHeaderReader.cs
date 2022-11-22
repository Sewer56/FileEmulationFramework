using System.Runtime.InteropServices;
using AWB.Stream.Emulator.Awb.Structs;
using AWB.Stream.Emulator.System.IO.Hashing;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace AWB.Stream.Emulator.Awb;

/// <summary>
/// Utility class that can be used to read out the AFS header from a given stream.
/// </summary>
public static unsafe class AwbHeaderReader
{
    /// <summary>
    /// Gets length of header of AWB file.
    /// </summary>
    /// <param name="stream">Stream to get length from. Stream is read but not advanced.</param>
    public static int GetHeaderLength(global::System.IO.Stream stream)
    {
        var pos = stream.Position;
        try
        {
            var header = stream.Read<Afs2Header>();
            return header.GetTotalSizeOfHeader();
        }
        finally
        {
            stream.Position = pos;
        }
    }
    
    /// <summary>
    /// Gets length of header of AWB file.
    /// </summary>
    /// <returns>False if the stream does not contain an AFS2 file or the stream could not be read to end.</returns>
    public static bool TryReadHeader(global::System.IO.Stream stream, out byte[]? data)
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

    /// <summary>
    /// Reads the header of an AWB file and hashes it.
    /// </summary>
    /// <param name="handle">Native handle to AWB file.</param>
    /// <param name="hash">Hash of said file.</param>
    /// <returns>False if hash unsuccessful.</returns>
    public static bool TryHashHeader(IntPtr handle, out ulong hash)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = stream.Position;
        try
        {
            return TryHashHeader(stream, out hash);
        }
        finally
        {
            stream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
    
    /// <summary>
    /// Reads the header of an AFS file and hashes it.
    /// </summary>
    /// <returns>False if hash unsuccessful.</returns>
    public static bool TryHashHeader(global::System.IO.Stream stream, out ulong hash)
    {
        hash = 0;
        if (!stream.TryRead(out Afs2Header header, out _))
            return false;
        
        // Get header size
        var headerSize = header.GetTotalSizeOfHeader();
        Span<byte> data = stackalloc byte[headerSize];

        MemoryMarshal.Cast<byte, Afs2Header>(data)[0] = header; // Write to start of array.
        if (!stream.TryRead(data[sizeof(Afs2Header)..], out _))
            return false;

        XxHash3.Hash(data, MemoryMarshal.Cast<ulong, byte>(MemoryMarshal.CreateSpan(ref hash, 1)));
        return true;
    }
    
    /// <summary>
    /// Hashes the header of an AFS file from memory.
    /// </summary>
    /// <returns>False if hash unsuccessful.</returns>
    public static ulong HashHeaderInRam(Afs2Header* header)
    {
        // Get header size & span
        var headerSize = header->GetTotalSizeOfHeader();
        var headerSpan = new Span<byte>((byte*)header, headerSize);
        ulong hash = 0;
        XxHash3.Hash(headerSpan, MemoryMarshal.Cast<ulong, byte>(MemoryMarshal.CreateSpan(ref hash, 1)));
        return hash;
    }
}