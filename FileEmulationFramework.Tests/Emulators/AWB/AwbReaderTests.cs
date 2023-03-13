using System;
using System.IO;
using AWB.Stream.Emulator.Awb;
using AWB.Stream.Emulator.Awb.Structs;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AWB;

/// <summary>
/// Tests for the AWB Reader.
/// </summary>
public class AwbReaderTests
{
    [Fact]
    public unsafe void ReadsCorrectHeaderData()
    {
        // Arrange Header
        using var stream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open, FileAccess.ReadWrite);
        AwbHeaderReader.TryReadHeader(stream, out var data);
        
        // Act
        fixed (byte* dataPtr = &data[0])
        {
            var viewer = AwbViewer.FromMemory(dataPtr);
            Span<FileEntry> files = stackalloc FileEntry[viewer.FileCount];
            viewer.GetEntries(files);
            
            Assert.Equal(0, files[0].Id);
            Assert.Equal(64, files[0].Position);
            
            Assert.Equal(1, files[1].Id);
            Assert.Equal(78944, files[1].Position);
            
            Assert.Equal(2, files[2].Id);
            Assert.Equal(149248, files[2].Position);
            
            Assert.Equal(5, files.Length);
        }
    }
}