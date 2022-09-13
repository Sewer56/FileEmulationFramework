using System.Collections.Generic;
using System.IO;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Tests.Extensions;
using Xunit;

namespace FileEmulationFramework.Tests;

public class MultiStreamTests
{
    [Fact]
    public void Read_Baseline()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(Assets.StreamTestFile));
        using var multiStream = new MultiStream(new List<StreamOffsetPair<Stream>>()
        {
            new(fileSliceStream, fileSliceStream.Slice.ToOffsetRange())
        });

        Assert.Equal(0, multiStream.ReadByte());
        Assert.Equal(1, multiStream.ReadByte());
        Assert.Equal(2, multiStream.ReadByte());
        Assert.Equal(3, multiStream.ReadByte());
    }

    [Fact]
    public void Read_CanSwitchStreams_AndReadFullStream()
    {
        using var sliceOne = new FileSliceStreamFs(new FileSlice(0, 4, Assets.StreamTestFile));
        using var sliceTwo = new FileSliceStreamFs(new FileSlice(4, 4, Assets.StreamTestFile));

        using var multiStream = new MultiStream(new List<StreamOffsetPair<Stream>>()
        {
            new(sliceOne, sliceOne.Slice.ToOffsetRange()),
            new(sliceTwo, sliceTwo.Slice.ToOffsetRange())
        });

        // Stream 1
        Assert.Equal(0, multiStream.ReadByte());
        Assert.Equal(1, multiStream.ReadByte());
        Assert.Equal(2, multiStream.ReadByte());
        Assert.Equal(3, multiStream.ReadByte());

        // Stream 2
        Assert.Equal(4, multiStream.ReadByte());
        Assert.Equal(5, multiStream.ReadByte());
        Assert.Equal(6, multiStream.ReadByte());
        Assert.Equal(7, multiStream.ReadByte());
    }

    [Fact]
    public void Read_CanReadAcrossStreamBounds()
    {
        using var sliceOne = new FileSliceStreamFs(new FileSlice(0, 4, Assets.StreamTestFile));
        using var sliceTwo = new FileSliceStreamFs(new FileSlice(4, 4, Assets.StreamTestFile));

        using var multiStream = new MultiStream(new List<StreamOffsetPair<Stream>>()
        {
            new(sliceOne, sliceOne.Slice.ToOffsetRange()),
            new(sliceTwo, sliceTwo.Slice.ToOffsetRange())
        });

        multiStream.Seek(2, SeekOrigin.Begin);

        // Read Half from Stream 1 & Half from Stream 2
        Assert.Equal(0x05040302, multiStream.Read<int>());
    }
}