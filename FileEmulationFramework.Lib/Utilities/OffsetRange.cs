using System.Diagnostics.CodeAnalysis;

namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Defines a physical offset range with a minimum and maximum address.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Known good implementation from Reloaded.Memory.Buffers")]
public struct OffsetRange
{
    /// <summary>
    /// Represents the first byte of the offset.
    /// </summary>
    public nint Start;

    /// <summary>
    /// Represents the last byte of the offset.
    /// </summary>
    public nint End;

    /// <summary>
    /// Creates an offset range from a given start and end offset.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="end">The end offset.</param>
    public OffsetRange(nint start, nint end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates an offset range from a given start and end offset.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="end">The end offset.</param>
    public static OffsetRange FromStartAndEnd(nint start, nint end) => new OffsetRange(start, end);

    /// <summary>
    /// Creates an offset range from a given start and length.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="length">Length of the offset range.</param>
    public static OffsetRange FromStartAndLength(nint start, nint length) => new OffsetRange(start, start + length);

    /// <summary>
    /// Returns true if the other address range is completely inside
    /// the current address range.
    /// </summary>
    public bool Contains(ref OffsetRange otherRange)
    {
        if (otherRange.Start >= this.Start &&
            otherRange.End <= this.End)
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
    private bool PointInRange(ref OffsetRange range, nint point)
    {
        if (point >= range.Start &&
            point <= range.End)
            return true;

        return false;
    }
}

/// <summary>
/// Defines a physical offset range with a minimum and maximum address.
/// This is the variant which uses 64-bit integers. Use this for files that can be bigger than 2GB.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Known good implementation from Reloaded.Memory.Buffers")]
public struct OffsetRangeLong
{
    /// <summary>
    /// Represents the first byte of the offset.
    /// </summary>
    public long Start;

    /// <summary>
    /// Represents the last byte of the offset.
    /// </summary>
    public long End;

    /// <summary>
    /// Creates an offset range from a given start and end offset.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="end">The end offset.</param>
    public OffsetRangeLong(long start, long end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates an offset range from a given start and end offset.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="end">The end offset.</param>
    public static OffsetRangeLong FromStartAndEnd(long start, long end) => new OffsetRangeLong(start, end);

    /// <summary>
    /// Creates an offset range from a given start and length.
    /// </summary>
    /// <param name="start">The start offset.</param>
    /// <param name="length">Length of the offset range.</param>
    public static OffsetRangeLong FromStartAndLength(long start, long length) => new OffsetRangeLong(start, start + length);
    
    /// <summary>
    /// Returns true if the other address range is completely inside
    /// the current address range.
    /// </summary>
    public bool Contains(ref OffsetRangeLong otherRange)
    {
        if (otherRange.Start >= this.Start &&
            otherRange.End <= this.End)
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if the other address range intersects another address range, i.e.
    /// start or end of this range falls inside other range.
    /// </summary>
    public bool Overlaps(ref OffsetRangeLong otherRange)
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
    private bool PointInRange(ref OffsetRangeLong range, long point)
    {
        if (point >= range.Start &&
            point <= range.End)
            return true;

        return false;
    }
}