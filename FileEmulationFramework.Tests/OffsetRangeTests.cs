using FileEmulationFramework.Lib.Utilities;
using Xunit;

namespace FileEmulationFramework.Tests;

/// <summary>
/// Tests related to offset ranges.
/// </summary>
public class OffsetRangeTests
{
    [Fact]
    public void IsAllJoined_BaseLine()
    {
        var offsets = new OffsetRange[]
        {
            new OffsetRange(0, 16),
            new OffsetRange(16, 32),
            new OffsetRange(32, 64),
            new OffsetRange(64, 100)
        };

        Assert.True(OffsetRangeExtensions.AreAllJoined(offsets));
    }

    [Fact]
    public void IsAllJoined_OnOffByOneError_Fails()
    {
        var offsets = new OffsetRange[]
        {
            new OffsetRange(0, 16),
            new OffsetRange(15, 31)
        };

        Assert.False(OffsetRangeExtensions.AreAllJoined(offsets));
    }

    [Fact]
    public void IsAllJoined_OnOverlap_Fails()
    {
        var offsets = new OffsetRange[]
        {
            new OffsetRange(0, 15),
            new OffsetRange(14, 31)
        };

        Assert.False(OffsetRangeExtensions.AreAllJoined(offsets));
    }

    [Fact]
    public void IsAllJoined_WithGapOnStart_Fails()
    {
        var offsets = new OffsetRange[]
        {
            new OffsetRange(1, 15),
            new OffsetRange(15, 31)
        };

        Assert.False(OffsetRangeExtensions.AreAllJoined(offsets));
    }

    [Fact]
    public void IsAllJoined_WithGapBetweenItems_Fails()
    {
        var offsets = new OffsetRange[]
        {
            new OffsetRange(0, 15),
            new OffsetRange(17, 31)
        };

        Assert.False(OffsetRangeExtensions.AreAllJoined(offsets));
    }
}