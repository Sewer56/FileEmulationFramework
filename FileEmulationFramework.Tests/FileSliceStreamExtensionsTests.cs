using System.Collections.Generic;
using System.IO;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Xunit;

namespace FileEmulationFramework.Tests;

public class FileSliceStreamExtensionsTests
{
    [Fact]
    public void MergeList_Success()
    {
        var streams = new List<StreamOffsetPair<Stream>>()
        {
            new (new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(0, 16)),
            new (new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(16, 32))
        };

        var merged = FileSliceStreamExtensions.MergeStreams(streams);
        Assert.Single(merged);
        Assert.Equal(48, merged[0].Offset.Length);
    }

    [Fact]
    public void MergeList_WithMultiMerge_Success()
    {
        var streams = new List<StreamOffsetPair<Stream>>()
        {
            new (new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(0, 16)),
            new (new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(16, 32)),
            new (new FileSliceStreamW32(new FileSlice(48, 32, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(48, 32)),
            new (new FileSliceStreamW32(new FileSlice(80, 32, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(80, 32)),
        };

        var merged = FileSliceStreamExtensions.MergeStreams(streams);
        Assert.Single(merged);
        Assert.Equal(112, merged[0].Offset.Length);
    }

    [Fact]
    public void MergeList_WithOtherStream_Success()
    {
        var streams = new List<StreamOffsetPair<Stream>>()
        {
            new (new MemoryStream(), OffsetRange.FromStartAndLength(48, 16)),
            new (new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(0, 16)),
            new (new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(16, 32)),
        };

        var merged = FileSliceStreamExtensions.MergeStreams(streams);
        Assert.Equal(2, merged.Count);
        Assert.Equal(48, merged[0].Offset.Length);
    }

    [Fact]
    public void MergeList_WithOtherStreamInBetween_Success()
    {
        var streams = new List<StreamOffsetPair<Stream>>()
        {
            new (new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(0, 16)),
            new (new MemoryStream(), OffsetRange.FromStartAndLength(16, 16)),
            new (new FileSliceStreamW32(new FileSlice(32, 32, Assets.StreamTestFile)), OffsetRange.FromStartAndLength(32, 32)),
        };

        var merged = FileSliceStreamExtensions.MergeStreams(streams);
        Assert.Equal(3, merged.Count);
        Assert.Equal(16, merged[0].Offset.Length);
        Assert.Equal(32, merged[2].Offset.Length);
    }

    [Fact]
    public void Merge_WithoutGap_Success()
    {
        var streams = new List<IFileSliceStream>()
        {
            new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)),
            new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)),
        };

        Assert.True(FileSliceStreamExtensions.TryMerge(streams[0], streams[1], out var result));
        Assert.Equal(48, result!.Length);
    }

    [Fact]
    public void Merge_WrongOrder_Success()
    {
        var streams = new List<IFileSliceStream>()
        {
            new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)),
            new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)),
        };

        Assert.True(FileSliceStreamExtensions.TryMerge(streams[1], streams[0], out var result));
        Assert.Equal(48, result.Length);
    }

    [Fact]
    public void Merge_Withgap_Failure()
    {
        var streams = new List<IFileSliceStream>()
        {
            new FileSliceStreamW32(new FileSlice(0, 15, Assets.StreamTestFile)),
            new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)),
        };

        Assert.False(FileSliceStreamExtensions.TryMerge(streams[1], streams[0], out _));
    }

    [Fact]
    public void Merge_WithOverlap_Failure()
    {
        var streams = new List<IFileSliceStream>()
        {
            new FileSliceStreamW32(new FileSlice(0, 17, Assets.StreamTestFile)),
            new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFile)),
        };

        Assert.False(FileSliceStreamExtensions.TryMerge(streams[1], streams[0], out _));
    }

    [Fact]
    public void Merge_DifferentHandle_Failure()
    {
        var streams = new List<IFileSliceStream>()
        {
            new FileSliceStreamW32(new FileSlice(0, 16, Assets.StreamTestFile)),
            new FileSliceStreamW32(new FileSlice(16, 32, Assets.StreamTestFileReverse)),
        };

        Assert.False(FileSliceStreamExtensions.TryMerge(streams[1], streams[0], out _));
    }
}