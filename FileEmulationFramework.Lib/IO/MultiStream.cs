using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;

namespace FileEmulationFramework.Lib.IO;

/// <summary>
/// A stream type that concatenates multiple external streams into one.
/// </summary>
public sealed class MultiStream : Stream
{
    private readonly Logger? _log;

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

    private Stream[] _streams;
    private OffsetRange[] _offsets;
    private OffsetRangeSelector _offsetRangeSelector;

    /// <summary>
    /// Stream that encapsulates 
    /// </summary>
    /// <param name="streamOffsetPair">
    ///     Pairs of streams and their corresponding offsets.
    ///     Can be specified in any order, but all offsets must link up to match
    ///     complete stream, leaving no gaps.
    /// 
    ///     i.e. 0-15, 16-32, 33-64 etc.
    /// </param>
    /// <param name="log">Optional logger used for logging.</param>
    public MultiStream(ICollection<StreamOffsetPair<Stream>> streamOffsetPair, Logger? log = null)
    {
        _log = log;

        // Ensure no overlap.
        _streams = GC.AllocateUninitializedArray<Stream>(streamOffsetPair.Count);
        _offsets = GC.AllocateUninitializedArray<OffsetRange>(streamOffsetPair.Count);

        // Copy Items
        int itemIndex = 0;
        foreach (var item in streamOffsetPair.OrderBy(x => x.Offset.Start))
        {
            _streams[itemIndex] = item.Stream;
            _offsets[itemIndex] = item.Offset;
            itemIndex++;
        }

        Length = _offsets[^1].End;
        _offsetRangeSelector = new OffsetRangeSelector(_offsets);
        if (!OffsetRangeExtensions.AreAllJoined(_offsets))
            ThrowHelpers.Argument("The provided stream & offset pairs don't form a complete file.\n" +
                                           "There are either gaps or overlaps in the ranges.");
    }

    /// <inheritdoc />
    public override void Flush() { }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        var index  = _offsetRangeSelector.Select(Position);
        if (index == -1)
        {
            _log?.Warning($"[{nameof(MultiStream)}] Cannot read from this position!! Position: {{0}}, Length: {{1}}. This is not necessarily an error, some implementations of e.g. CopyTo might cause this; just be weary of this message.", Position, Length);
            return 0;
        }

        var stream = _streams[index];

        var numBytesRead = 0;
        var numToRead = buffer.Length;

        while (numToRead > 0)
        {
            // Seek where needed in stream.
            ref var currentOffset = ref _offsets[index];
            var offsetInStream = Position - currentOffset.Start;
            
            stream.Seek(offsetInStream, SeekOrigin.Begin);
            var toReadFromThisStream = Math.Min(numToRead, (int)((currentOffset.End) - Position));

            // Read
            bool success = stream.TryRead(buffer.Slice(numBytesRead, toReadFromThisStream), out var numReadFromThisStream);
            
            // Adjust pointers.
            Position += numReadFromThisStream;
            numBytesRead += numReadFromThisStream;
            numToRead -= numReadFromThisStream;

            if (success && numToRead > 0 && index < _offsets.Length - 1)
            {
                // Advance to next stream if necessary.
                stream = _streams[++index];
                _offsetRangeSelector.LastIndex = index;
            }
            else
                return numBytesRead;
        }

        return numBytesRead;
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
        
        return Position;
    }

    /// <inheritdoc />
    public override void SetLength(long value) { }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpanFast(offset, count));
}