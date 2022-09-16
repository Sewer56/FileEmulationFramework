using FileEmulationFramework.Lib.Utilities;

namespace FileEmulationFramework.Lib.IO.Struct;

/// <summary>
/// A struct that represents a pair between a stream and an offset range.
/// Used as input.
/// </summary>
public struct StreamOffsetPair<TStream> where TStream : Stream
{
    /// <summary>
    /// The stream in question.
    /// </summary>
    public TStream Stream;

    /// <summary>
    /// The offset associated with the stream.
    /// </summary>
    public OffsetRange Offset;

    /// <summary>
    /// Pairs a stream and an offset range.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="offset">The offset range.</param>
    public StreamOffsetPair(TStream stream, OffsetRange offset)
    {
        Stream = stream;
        Offset = offset;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Offset}, {Stream}";
}