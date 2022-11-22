using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace FileEmulationFramework.Lib.IO;

/// <summary>
/// Represents a stream that is backed by the slice of a file.
/// Supports read and seek operations. Resizes and/or writes are not supported.
/// This variant is backed by FileStream.
/// </summary>
public class FileSliceStreamFs : Stream, IFileSliceStream
{
    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => true;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length { get; }

    /// <inheritdoc />
    public override long Position { get; set; }

    /// <summary>
    /// The backing file slice for this stream.
    /// </summary>
    public FileSlice Slice { get; private set; }

    /// <summary>
    /// Underlying file stream.
    /// </summary>
    private FileStream _stream;
    private Logger? _logger;

    /// <summary>
    /// Creates a stream backed by the slice of a file.
    /// </summary>
    /// <param name="slice">The slice of file to use as backing.</param>
    /// <param name="logger">The logger to provide error logging to.</param>
    public FileSliceStreamFs(FileSlice slice, Logger? logger = null)
    {
        Slice  = slice;
        Length = slice.Length;
        _logger = logger;
        _stream = new FileStream(new SafeFileHandle(slice.Handle, false), FileAccess.Read);
        _stream.Position = slice.Offset;
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        var maxToRead = Math.Min(buffer.Length, Length - Position);
        bool success = _stream.TryRead(buffer.Slice(0, (int)maxToRead), out int numRead);
        if (!success && _logger != null) 
            ReportReadError(maxToRead);

        Position += numRead;
        return numRead;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length + offset;
                break;
        }

        _stream.Position = Slice.Offset + Position;
        return Position;
    }

    /// <summary>
    /// This is a no-op.
    /// </summary>
    public override void Flush() { }

    /// <summary>
    /// Sets the length of the underlying stream.
    /// This value is ignored.
    /// </summary>
    /// <param name="value">This value is ignored.</param>
    public override void SetLength(long value) { }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotImplementedException("This stream only supports reading.");

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpanFast(offset, count));

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpanFast(offset, count));

    private void ReportReadError(long maxToRead)
    {
        _logger!.Error($"[{nameof(FileSliceStreamFs)}] Failed to read all requested bytes. This isn't good.\n" +
                       "Current Pos: {0}, Num To Read: {1}\n" +
                       "Slice: {2}", Position, maxToRead, Slice);
    }
}