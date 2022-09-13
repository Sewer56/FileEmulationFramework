using BenchmarkDotNet.Attributes;
using FileEmulationFramework.Lib.Utilities;

namespace FileEmulationFramework.Benchmarks;

/// <summary>
/// Calculates the average time it takes to select a single offset using the offset selector.
/// </summary>
public class OffsetSelectionBenchmarks
{
    [Params(4, 8, 16, 64, 256, 1024, 4096, 16384)]
    public int NumOffsets { get; set; }

    const int OffsetBetweenItems = 100;

    /// <summary>
    /// Individual offset ranges.
    /// </summary>
    public OffsetRange[] Ranges { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        Ranges = new OffsetRange[NumOffsets];
        int currentOffset = 0;
        
        for (int x = 0; x < Ranges.Length; x++)
        {
            Ranges[x] = new OffsetRange(currentOffset, currentOffset + OffsetBetweenItems - 1);
            currentOffset += OffsetBetweenItems;
        }
    }

    [Benchmark]
    public void BinarySearch()
    {
        var selector = new OffsetRangeSelector(Ranges);
        int currentOffset = 1;
        for (int x = 0; x < NumOffsets; x++)
        {
            selector.SelectBinarySearch(currentOffset);
            currentOffset += OffsetBetweenItems;
        }
    }

    [Benchmark]
    public void ForSearch()
    {
        var selector = new OffsetRangeSelector(Ranges);
        int currentOffset = 1;
        for (int x = 0; x < NumOffsets; x++)
        {
            selector.SelectLoop(currentOffset);
            currentOffset += OffsetBetweenItems;
        }
    }
}