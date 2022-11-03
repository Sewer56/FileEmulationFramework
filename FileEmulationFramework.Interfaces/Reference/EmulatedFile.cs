namespace FileEmulationFramework.Interfaces.Reference;

/// <summary>
/// Reference implementation for representing an emulated file using a (MultiStream); good enough if you don't need any custom behaviour.
/// </summary>
public class EmulatedFile<TStream> : IEmulatedFile where TStream : Stream
{
    /// <summary>
    /// The stream this class is based on.
    /// </summary>
    public TStream BaseStream { get; set; }
    
    /// <summary>
    /// Creates an emulated file from a stream.
    /// </summary>
    /// <param name="baseStream">Stream from which this emulated file is based from.</param>
    public EmulatedFile(TStream baseStream) => BaseStream = baseStream;

    /// <inheritdoc/>
    public long GetFileSize(IntPtr handle, IFileInformation info) => BaseStream.Length;

    /// <inheritdoc/>
    public unsafe bool ReadData(IntPtr handle, byte* buffer, uint length, long offset, IFileInformation info, out int numReadBytes)
    {
        var bufferSpan = new Span<byte>(buffer, (int)length);
        BaseStream.Seek(offset, SeekOrigin.Begin);
        numReadBytes = BaseStream.Read(bufferSpan);
        return numReadBytes > 0;
    }

    /// <inheritdoc/>
    public void CloseHandle(IntPtr handle, IFileInformation info) { }
}