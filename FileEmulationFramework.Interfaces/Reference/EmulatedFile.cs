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
    /// The last time the file was written to.
    /// If null, the last write time of the original file will be reported.
    /// </summary>
    public DateTime? LastWrite { get; set; }
    
    /// <summary>
    /// Creates an emulated file from a stream.
    /// This file will report the last write time of the original.
    /// </summary>
    /// <param name="baseStream">Stream from which this emulated file is based from.</param>
    public EmulatedFile(TStream baseStream) => BaseStream = baseStream;


    /// <summary>
    /// Creates an emulated file from a stream with a last write time.
    /// </summary>
    /// <param name="baseStream">Stream from which this emulated file is based from.</param>
    /// <param name="lastWrite">The last write time that the emulated file will report.</param>
    public EmulatedFile(TStream baseStream, DateTime lastWrite)
    {
        BaseStream = baseStream;
        LastWrite = lastWrite;
    }
    
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

    /// <inheritdoc/>
    public bool TryGetLastWriteTime(IntPtr handle, IFileInformation info, out DateTime? lastWriteTime)
    {
        lastWriteTime = LastWrite;
        return lastWriteTime != null;
    }
}