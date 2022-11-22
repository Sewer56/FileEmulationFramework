using System;
using System.IO;
using FileEmulationFramework.Lib.IO;
using Xunit;

namespace FileEmulationFramework.Tests;

/// <summary>
/// Unit tests related to file slices.
/// </summary>
public class FileSliceTests
{
    /// <summary>
    /// Tests the <see cref="FileSlice.GetData(System.Span{byte})"/> method.
    /// </summary>
    [Fact]
    public void GetData_MatchesFramework()
    {
        var expectedBytes = File.ReadAllBytes(Assets.StreamTestFile);

        var fileSlice = new FileSlice(Assets.StreamTestFile);
        fileSlice.GetData(out var bytes);

        for (int x = 0; x < expectedBytes.Length; x++)
            Assert.Equal(expectedBytes[x], bytes[x]);
    }
    
    [Fact]
    public void SliceUpTo_CapsAtFileLength()
    {
        var fileSlice = new FileSlice(Assets.StreamTestFile);
        var length = fileSlice.Length;

        var sliced = fileSlice.SliceUpTo(0, length + 1);
        Assert.Equal(length, sliced.Length);
    }
    
    [Fact]
    public void SliceUpTo_ThrowsAtNegative()
    {
        var fileSlice = new FileSlice(Assets.StreamTestFile);
        Assert.Throws<ArgumentException>(() => fileSlice.SliceUpTo(-1, 0));
        Assert.Throws<ArgumentException>(() => fileSlice.SliceUpTo(0, -1));
    }

    [Fact]
    public void Slice_ThrowsAtNegative()
    {
        var fileSlice = new FileSlice(Assets.StreamTestFile);
        Assert.Throws<ArgumentException>(() => fileSlice.Slice(-1, 0));
        Assert.Throws<ArgumentException>(() => fileSlice.Slice(0, -1));
    }

    [Fact]
    public void Slice_ThrowsAtZeroLength()
    {
        var fileSlice = new FileSlice(16, 16, Assets.StreamTestFile);
        Assert.Throws<ArgumentException>(() => fileSlice.Slice(16, 0));
    }

    [Fact]
    public void SliceUpTo_ThrowsAtZeroLength()
    {
        var fileSlice = new FileSlice(16, 16, Assets.StreamTestFile);
        Assert.Throws<ArgumentException>(() => fileSlice.SliceUpTo(16, 0));
    }

    [Fact]
    public void Slice_ThrowsPastFileLength()
    {
        var fileSlice = new FileSlice(Assets.StreamTestFile);
        var length = fileSlice.Length;
        Assert.Throws<ArgumentException>(() => fileSlice.Slice(0, length + 1));

        // Does not throw
        fileSlice.Slice(0, length);
    }

    [Fact]
    public void Merge_WithoutGap_Success()
    {
        var first  = new FileSlice(0, 16, Assets.StreamTestFile);
        var second = new FileSlice(16, 32, Assets.StreamTestFile);

        Assert.True(FileSlice.TryMerge(first, second, out var result));
        Assert.Equal(48, result!.Length);
    }

    [Fact]
    public void Merge_WrongOrder_Success()
    {
        var first = new FileSlice(0, 16, Assets.StreamTestFile);
        var second = new FileSlice(16, 32, Assets.StreamTestFile);

        Assert.True(FileSlice.TryMerge(second, first, out _));
    }

    [Fact]
    public void Merge_Withgap_Failure()
    {
        var first = new FileSlice(0, 15, Assets.StreamTestFile);
        var second = new FileSlice(16, 32, Assets.StreamTestFile);

        Assert.False(FileSlice.TryMerge(first, second, out _));
    }

    [Fact]
    public void Merge_WithOverlap_Failure()
    {
        var first = new FileSlice(0, 17, Assets.StreamTestFile);
        var second = new FileSlice(16, 32, Assets.StreamTestFile);

        Assert.False(FileSlice.TryMerge(first, second, out _));
    }

    [Fact]
    public void Merge_DifferentHandle_Failure()
    {
        var first = new FileSlice(0, 16, Assets.StreamTestFile);
        var second = new FileSlice(16, 32, Assets.StreamTestFileReverse);

        Assert.False(FileSlice.TryMerge(first, second, out _));
    }
}