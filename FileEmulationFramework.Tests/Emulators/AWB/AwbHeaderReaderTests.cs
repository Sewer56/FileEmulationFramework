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
        using var stream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open, FileAccess.ReadWrite);
        Assert.True(AwbHeaderReader.TryReadHeader(stream, out var data));
        Assert.Equal(50, data.Length);
    }
}