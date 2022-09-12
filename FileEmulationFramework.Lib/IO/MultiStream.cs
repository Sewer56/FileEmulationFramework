using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;

namespace FileEmulationFramework.Lib.IO;

/// <summary>
/// A stream type that concatenates multiple external streams into one.
/// </summary>
public class MultiStream : Stream
{
    /// <inheritdoc />
    public override bool CanRead { get; } = true;

    /// <inheritdoc />
    public override bool CanSeek { get; } = true;

    /// <inheritdoc />
    public override bool CanWrite { get; } = false;

    /// <inheritdoc />
    public override long Length { get; }

    /// <inheritdoc />
    public override long Position { get; set; } = 0;

    private Stream[] _streams;
    private OffsetRange[] _offsets;

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
    public MultiStream(ICollection<StreamOffsetPair> streamOffsetPair)
    {
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

        Length = _offsets[^0].End + 1;
        if (!OffsetRangeExtensions.AreAllJoined(_offsets))
            ThrowHelpers.ArgumentException("The provided stream & offset pairs don't form a complete file.\n" +
                                           "There are either gaps or overlaps in the ranges.");
    }

    /// <inheritdoc />
    public override void Flush() { }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void SetLength(long value) { }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
}