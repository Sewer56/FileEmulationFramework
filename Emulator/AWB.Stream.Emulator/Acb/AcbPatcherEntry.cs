using AWB.Stream.Emulator.Awb;

namespace AWB.Stream.Emulator.Acb;

/// <summary>
/// Represents individual entry in ACB Patcher.
/// This entry contains stream and length/offset pair for AWB header.
/// </summary>
public struct AcbPatcherEntry
{
    /// <summary>
    /// The stream containing the data.
    /// </summary>
    public global::System.IO.Stream Stream;
    
    // Header should be at start of file so int is enough.
    
    /// <summary>
    /// Offset of the data.
    /// </summary>
    public int Offset;
    
    /// <summary>
    /// Length of the header data in contained stream.
    /// </summary>
    public int DataLength;

    /// <summary>
    /// Path of the AWB file tied to this ACB entry.
    /// </summary>
    public string AwbPath;

    /// <summary>
    /// Creates a patcher entry from the stream.
    /// </summary>
    /// <param name="stream">Stream that starts with AWB header. Stream is not advanced.</param>
    /// <param name="awbPath">Path of the AWB file tied to this patcher entry.</param>
    /// <returns>The entry to the ACB Patcher.</returns>
    public static AcbPatcherEntry FromAwbStream(global::System.IO.Stream stream, string awbPath)
    {
        var offset = stream.Position;
        var length = AwbHeaderReader.GetHeaderLength(stream);
        return new AcbPatcherEntry()
        {
            Offset = (int)offset,
            DataLength = length,
            Stream = stream,
            AwbPath = awbPath
        };
    }

    /// <summary>
    /// Writes the contents of the stream to a given address.
    /// </summary>
    /// <param name="ptr">The address.</param>
    public unsafe void WriteToAddress(byte* ptr)
    {
        Stream.Position = Offset;
        Stream.ReadAtLeast(new Span<byte>(ptr, DataLength), DataLength);
    }
};