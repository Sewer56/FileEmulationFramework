using System.IO;
using AWB.Stream.Emulator.Awb;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AWB;

/// <summary>
/// Tests for the AWB header Reader utility.
/// </summary>
public class AwbHeaderReaderTests
{
    [Fact]
    public void ReadsCorrectHeaderSize()
    {
        var stream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open);
        Assert.True(AwbHeaderReader.TryReadHeader(stream, out var data));
        Assert.Equal(64, data.Length);
    }
}