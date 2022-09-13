using System.IO;
using FileEmulationFramework.Lib.IO;
using Xunit;

namespace FileEmulationFramework.Tests;

public class FileSliceStreamTests
{
    // Note: File has current offset stored in each byte for easier testing.
    private const int FileLength = 256;

    /// <summary>
    /// Offset for those tests that use a file slice.
    /// </summary>
    private const int SliceOffset = 16;

    [Fact]
    public void Read_FromStart_Baseline_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(Assets.StreamTestFile));
        NewMethodRead_FromStart_Baseline_Generic(fileSliceStream);
    }

    [Fact]
    public void Read_FromStart_Baseline_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(Assets.StreamTestFile));
        NewMethodRead_FromStart_Baseline_Generic(fileSliceStream);
    }

    private static void NewMethodRead_FromStart_Baseline_Generic(Stream fileSliceStream)
    {
        for (int x = 0; x <= byte.MaxValue; x++)
            Assert.Equal((byte)x, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Read_CannotBeyondEndOfFile_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(Assets.StreamTestFile));
        Read_CannotBeyondEndOfFile_Generic(fileSliceStream);
    }

    [Fact]
    public void Read_CannotBeyondEndOfFile_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(Assets.StreamTestFile));
        Read_CannotBeyondEndOfFile_Generic(fileSliceStream);
    }

    private static void Read_CannotBeyondEndOfFile_Generic(Stream fileSliceStream)
    {
        fileSliceStream.Seek(FileLength - 1, SeekOrigin.Begin);
        Assert.NotEqual(-1, fileSliceStream.ReadByte()); // End of file.
        Assert.Equal(-1, fileSliceStream.ReadByte()); // End of file.
    }

    [Fact]
    public void Seek_Current_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(Assets.StreamTestFile));
        Seek_Current_Generic(fileSliceStream);
    }

    [Fact]
    public void Seek_Current_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(Assets.StreamTestFile));
        Seek_Current_Generic(fileSliceStream);
    }

    private static void Seek_Current_Generic(Stream fileSliceStream)
    {
        fileSliceStream.Seek(8, SeekOrigin.Begin);
        fileSliceStream.Seek(4, SeekOrigin.Current);
        Assert.Equal(12, fileSliceStream.Position);
        Assert.Equal(12, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Seek_End_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(Assets.StreamTestFile));
        Seek_End_Generic(fileSliceStream);
    }

    [Fact]
    public void Seek_End_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(Assets.StreamTestFile));
        Seek_End_Generic(fileSliceStream);
    }

    private static void Seek_End_Generic(Stream fileSliceStream)
    {
        fileSliceStream.Seek(-4, SeekOrigin.End);
        Assert.Equal(FileLength - 4, fileSliceStream.Position);
        Assert.Equal(FileLength - 4, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Seek_Start_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(Assets.StreamTestFile));
        Seek_Start_Generic(fileSliceStream);
    }

    [Fact]
    public void Seek_Start_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(Assets.StreamTestFile));
        Seek_Start_Generic(fileSliceStream);
    }

    private static void Seek_Start_Generic(Stream fileSliceStream)
    {
        fileSliceStream.Seek(8, SeekOrigin.Begin);
        Assert.Equal(8, fileSliceStream.Position);
        Assert.Equal(8, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Seek_Start_WithFileSlice_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(SliceOffset, FileLength - SliceOffset, Assets.StreamTestFile));
        Seek_Start_WithFileSlice_Generic(fileSliceStream);
    }

    [Fact]
    public void Seek_Start_WithFileSlice_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(SliceOffset, FileLength - SliceOffset, Assets.StreamTestFile));
        Seek_Start_WithFileSlice_Generic(fileSliceStream);
    }

    private static void Seek_Start_WithFileSlice_Generic(Stream fileSliceStream)
    {
        Assert.Equal(SliceOffset, fileSliceStream.ReadByte());
        fileSliceStream.Seek(16, SeekOrigin.Begin);
        Assert.Equal(SliceOffset + 16, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Seek_End_WithFileSlice_Fs()
    {
        const int length = FileLength - (SliceOffset * 2);
        const int lastByte = length + SliceOffset;
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(SliceOffset, length, Assets.StreamTestFile));
        Seek_End_WithFileSlice_Generic(fileSliceStream, lastByte);
    }

    [Fact]
    public void Seek_End_WithFileSlice_W32()
    {
        const int length = FileLength - (SliceOffset * 2);
        const int lastByte = length + SliceOffset;
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(SliceOffset, length, Assets.StreamTestFile));
        Seek_End_WithFileSlice_Generic(fileSliceStream, lastByte);
    }

    private static void Seek_End_WithFileSlice_Generic(Stream fileSliceStream, int lastByte)
    {
        // Check last byte is correct.
        fileSliceStream.Seek(-1, SeekOrigin.End);
        Assert.Equal(lastByte - 1, fileSliceStream.ReadByte());

        // Seek 16 from end.
        fileSliceStream.Seek(-16, SeekOrigin.End);
        Assert.Equal(lastByte - 16, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Seek_Current_WithFileSlice_Fs()
    {
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(SliceOffset, FileLength - SliceOffset, Assets.StreamTestFile));
        Seek_Current_WithFileSlice_Generic(fileSliceStream);
    }

    [Fact]
    public void Seek_Current_WithFileSlice_W32()
    {
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(SliceOffset, FileLength - SliceOffset, Assets.StreamTestFile));
        Seek_Current_WithFileSlice_Generic(fileSliceStream);
    }

    private static void Seek_Current_WithFileSlice_Generic(Stream fileSliceStream)
    {
        // Check first byte is correct.
        Assert.Equal(SliceOffset, fileSliceStream.ReadByte());

        // Seek 16 from start.
        fileSliceStream.Seek(16, SeekOrigin.Begin);
        fileSliceStream.Seek(16, SeekOrigin.Current);

        // Ensure correct after seek.
        Assert.Equal(SliceOffset + 32, fileSliceStream.ReadByte());
    }

    [Fact]
    public void Read_CannotBeyondEndOfFile_WithFileSlice_Fs()
    {
        const int length = FileLength - (SliceOffset * 2);
        using var fileSliceStream = new FileSliceStreamFs(new FileSlice(SliceOffset, length, Assets.StreamTestFile));
        Read_CannotBeyondEndOfFile_WithFileSlice_Generic(fileSliceStream);
    }

    [Fact]
    public void Read_CannotBeyondEndOfFile_WithFileSlice_W32()
    {
        const int length = FileLength - (SliceOffset * 2);
        using var fileSliceStream = new FileSliceStreamW32(new FileSlice(SliceOffset, length, Assets.StreamTestFile));
        Read_CannotBeyondEndOfFile_WithFileSlice_Generic(fileSliceStream);
    }

    private static void Read_CannotBeyondEndOfFile_WithFileSlice_Generic(Stream fileSliceStream)
    {
        fileSliceStream.Seek(0, SeekOrigin.End);
        Assert.Equal(-1, fileSliceStream.ReadByte()); // End of file.
    }
}