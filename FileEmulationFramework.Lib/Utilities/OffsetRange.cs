namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Defines a physical offset range with a minimum and maximum address.
/// This is the variant which uses 64-bit integers. Use this for files that can be bigger than 2GB.
/// </summary>
public struct OffsetRange
{
    /// <summary>
    /// Represents the first byte of the offset.
    /// </summary>
    public long Start;

    /// <summary>
    /// Represents the end of the offset. [Exclusive]
    /// </summary>
    public long End;

    /// <summary>
    /// The length of this offset range.
    /// </summary>
    public long Length => End - Start;

    /// <summary>
    /// Creates an offset range from a given start and end offset.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="end">The end offset.</param>
    public OffsetRange(long start, long end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates an offset range from a given start and end offset.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="end">The end offset.</param>
    public static OffsetRange FromStartAndEnd(long start, long end) => new OffsetRange(start, end);

    /// <summary>
    /// Creates an offset range from a given start and length.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="length">Length of the offset range.</param>
    public static OffsetRange FromStartAndLength(long start, long length) => new OffsetRange(start, start + length);
    
    /// <summary>
    /// Returns true if the other address range is completely inside
    /// the current address range.
    /// </summary>
    public bool Contains(ref OffsetRange otherRange)
    {
        if (otherRange.Start >= this.Start &&
            otherRange.End < this.End)
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if the other address range intersects another address range, i.e.
    /// start or end of this range falls inside other range.
    /// </summary>
    public bool Overlaps(ref OffsetRange otherRange)
    {
        if (PointInRange(ref otherRange, this.Start))
            return true;

        if (PointInRange(ref otherRange, this.End))
            return true;

        if (PointInRange(ref this, otherRange.Start))
            return true;

        if (PointInRange(ref this, otherRange.End))
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if a number "point", is between min and max of address range.
    /// </summary>
    /// <param name="range">The range to check.</param>
    /// <param name="point">The offset to check if between <see cref="Start"/> [inclusive] and <see cref="End"/> [inclusive].</param>
    public static bool PointInRange(ref OffsetRange range, long point)
    {
        if (point >= range.Start && point < range.End)
            return true;

        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Start}-{End}";
}

/// <summary>
/// Extensions related to <see cref="OffsetRange"/>.
/// </summary>
public static class OffsetRangeExtensions
{
    /// <summary>
    /// Verifies whether all offset ranges are joined and form a complete range from 0 to end of last range.
    /// Assumes items are sorted by starting offset.
    /// </summary>
    /// <param name="ranges">Sorted collection (low to high) of ranges.</param>
    public static bool AreAllJoined(Span<OffsetRange> ranges)
    {
        // Ensure no gap at start of range.
        if (ranges[0].Start != 0)
            return false;

        for (int x = 0; x < ranges.Length - 1; x++)
        {
            // Check all pairs.
            var second = ranges[x + 1];
            var first  = ranges[x];

            if (!AreJoined(first, second)) 
                return false;

            // If end of first item matches start of other, there can't be an overlap,
            // since the very first item can't overlap with anything.

            // If this condition does not hold true, there is an overlap or gap between
            // two items.
        }

        return true;
    }

    /// <summary>
    /// Verifies whether the pair of offset ranges are joined
    /// </summary>
    /// <param name="first">The first address.</param>
    /// <param name="second">The second address</param>
    /// <param name="result">The resulting joined ranges</param>
    public static bool TryJoin(OffsetRange first, OffsetRange second, out OffsetRange result)
    {
        if (first.Start < second.Start)
            return TryJoinInternal(first, second, out result);

        return TryJoinInternal(second, first, out result);
    }

    /// <summary>
    /// Verifies whether the pair of offset ranges are joined
    /// </summary>
    /// <param name="first">The first address.</param>
    /// <param name="second">The second address</param>
    public static bool AreJoined(OffsetRange first, OffsetRange second)
    {
        if (first.Start < second.Start)
            return AreJoinedInternal(first, second);

        return AreJoinedInternal(second, first);
    }

    /// <summary>
    /// Verifies whether the pair of offset ranges are joined
    /// </summary>
    /// <param name="first">The first address.</param>
    /// <param name="second">The second address</param>
    /// <param name="result">The resulting joined ranges</param>
    private static bool TryJoinInternal(OffsetRange first, OffsetRange second, out OffsetRange result)
    {
        if (!AreJoinedInternal(first, second))
        {
            result = default;
            return false;
        }

        result = OffsetRange.FromStartAndLength(first.Start, second.End - first.Start);
        return true;
    }

    private static bool AreJoinedInternal(OffsetRange first, OffsetRange second)
    {
        return first.End == second.Start;
    }
}