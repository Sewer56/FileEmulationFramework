namespace AWB.Stream.Emulator.Awb.Structs;

/// <summary>
/// Represents an individual file held inside the library.
/// Custom struct, not part of AWB.
/// </summary>
public struct FileEntry
{
    /// <summary>
    /// ID of the entry.
    /// Variable length, length defined in header.
    /// </summary>
    public long Id;

    /// <summary>
    /// Position of the entry.
    /// </summary>
    public long Position;
    
    /// <summary>
    /// Length of the entry.
    /// </summary>
    public long Length;
}