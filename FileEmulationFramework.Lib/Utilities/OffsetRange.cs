using System.Diagnostics.CodeAnalysis;

namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Defines a physical offset range with a minimum and maximum address.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Known good implementation from Reloaded.Memory.Buffers")]
internal struct OffsetRange
{
    /// <summary>
    /// Represents the first byte of the offset.
    /// </summary>
    public int Start;

    /// <summary>
    /// Represents the last byte of the offset.
    /// </summary>
    public int End;

    public OffsetRange(int start, int end)
    {
        Start = start;
        End = end;
    }

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
    private bool PointInRange(ref OffsetRange range, long point)
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
internal struct OffsetRangeLong
{
    /// <summary>
    /// Represents the first byte of the offset.
    /// </summary>
    public long Start;

    /// <summary>
    /// Represents the last byte of the offset.
    /// </summary>
    public long End;

    public OffsetRangeLong(long start, long end)
    {
        Start = start;
        End = end;
    }

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