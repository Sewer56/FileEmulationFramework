using AWB.Stream.Emulator.Awb.Structs;
using AWB.Stream.Emulator.Awb.Utilities;
using FileEmulationFramework.Lib.Utilities;

namespace AWB.Stream.Emulator.Awb;

/// <summary>
/// Class for reading AWB metadata.
/// </summary>
public unsafe struct AwbViewer
{
    /// <summary>
    /// Pointer to the file header.
    /// </summary>
    public Afs2Header* Header;

    /// <summary>
    /// Total size of file.
    /// </summary>
    public long FileSize;

    /// <summary>
    /// Gets the number of files.
    /// </summary>
    public int FileCount => Header->EntryCount;
    
    /// <summary>
    /// Parses the AWB header data from existing memory.
    /// </summary>
    /// <param name="data">Data containing the AWB file/header.</param>
    /// <returns>Instance of the viewer.</returns>
    public static AwbViewer FromMemory(byte* data)
    {
        var viewer = new AwbViewer();
        viewer.Header = (Afs2Header*)data;
        viewer.Header->AssertSupportedArchive();

        var sizeOfEntryBytes = viewer.Header->GetSizeOfEntryBytes() * viewer.Header->EntryCount;
        var fileSizePtr = (data + sizeof(Afs2Header) + sizeOfEntryBytes);
        viewer.FileSize = ValueReaders.ReadNumber(fileSizePtr, viewer.Header->PositionFieldLength);
        
        return viewer;
    }

    /// <summary>
    /// Places all known file entries into the provided span.
    /// </summary>
    /// <param name="buffer">The buffer that will store all file entries.</param>
    /// <returns>Filled in buffer. If buffer is insufficient size, partially filled buffer.</returns>
    public Span<FileEntry> GetEntries(Span<FileEntry> buffer)
    {
        var numItems = Math.Min(buffer.Length, Header->EntryCount);
        var currentEntryPtr = (byte*)(Header + 1);
        
        for (int x = 0; x < numItems; x++)
            buffer[x].Id = Header->ReadIdFieldAndIncrementPtr(ref currentEntryPtr);
        
        // AFS2 files store unaligned position so you can calculate size between current and previous from offsets
        // but the actual data is written aligned.
        for (int x = 0; x < numItems; x++)
            buffer[x].Position = Mathematics.RoundUp(Header->ReadPositionAndIncrementPtr(ref currentEntryPtr), Header->Alignment);
        
        // Calculate length of entries
        for (int x = 0; x < buffer.Length - 1; x++)
            buffer[x].Length = buffer[x + 1].Position - buffer[x].Position;

        buffer[^1].Length = FileSize - buffer[^1].Position;
        return buffer.Slice(0, numItems);
    }
}