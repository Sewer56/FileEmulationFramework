using System.Diagnostics.CodeAnalysis;
using FileEmulationFramework.Lib.Utilities;

namespace FileEmulationFramework.Lib.IO;

/// <summary>
/// Stream that fills the read buffer with a single, user specified byte.
/// </summary>
public class PaddingStream : Stream
{
    /// <inheritdoc/>
    public override bool CanRead { get; } = true;

    /// <inheritdoc/>
    public override bool CanSeek { get; } = true;

    /// <inheritdoc/>
    public override bool CanWrite { get; } = false;

    /// <inheritdoc/>
    public override long Length => _length;

    /// <inheritdoc/>
    public override long Position { get; set; }

    private long _length;

    /// <summary>
    /// The value used as padding in this stream.
    /// </summary>
    public byte Value { get; private set; }

    /// <summary>
    /// Creates a stream that can be used as padding.
    /// </summary>
    /// <param name="value">The value used as padding in this stream.</param>
    /// <param name="length">The length of the stream.</param>
    public PaddingStream(byte value, int length)
    {
        _length = length;
        Value = value;
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpanFast(offset, count));

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        var maxToRead = Math.Min(buffer.Length, (int)(Length - Position));
        buffer[..maxToRead].Fill(Value);
        return maxToRead;
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage(Justification = "Standard implementation.")]
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
        
        return Position;
    }

    /// <inheritdoc/>
    public override void Flush() { }

    /// <inheritdoc/>
    public override void SetLength(long value) => _length = value;

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException("This stream only supports reading.");
}