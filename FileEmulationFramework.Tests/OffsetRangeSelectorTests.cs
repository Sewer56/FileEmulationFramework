using FileEmulationFramework.Lib.Utilities;
using Xunit;

namespace FileEmulationFramework.Tests;

public class OffsetRangeSelectorTests
{
    [Fact]
    public void BinarySearch_Baseline()
    {
        var selector = new OffsetRangeSelector(new OffsetRange[]
        {
            new (0, 15),
            new (16, 32),
            new (33, 64),
            new (64, 128),
        });

        Assert.Equal(0, selector.SelectLoop(0));
        Assert.Equal(1, selector.SelectLoop(16));
        Assert.Equal(2, selector.SelectLoop(45));
        Assert.Equal(3, selector.SelectLoop(100));

        Assert.Equal(0, selector.SelectBinarySearch(0));
        Assert.Equal(1, selector.SelectBinarySearch(16));
        Assert.Equal(2, selector.SelectBinarySearch(45));
        Assert.Equal(3, selector.SelectBinarySearch(100));
    }

    [Fact]
    public void BinarySearch_OnInvalidSearch_Fail()
    {
        var selector = new OffsetRangeSelector(new OffsetRange[]
        {
            new (0, 15),
            new (16, 32)
        });

        Assert.Equal(-1, selector.SelectLoop(33));
        Assert.Equal(-1, selector.SelectBinarySearch(33));
    }
}