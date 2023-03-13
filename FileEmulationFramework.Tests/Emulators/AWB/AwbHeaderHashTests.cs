using System.IO;
using AWB.Stream.Emulator.Awb;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AWB;

/// <summary>
/// Tests for hashing AWB Headers
/// </summary>
public class AwbHeaderHashTests
{
    /// <summary>
    /// Verifies hash algorithm works since we kinda stole xxHash3 from .NET 8
    /// </summary>
    [Fact]
    public void Baseline()
    {
        // Create Builder & Inject Single File
        using var stream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open, FileAccess.ReadWrite);
        Assert.True(AwbHeaderReader.TryHashHeader(stream, out var firstHash));
        stream.Seek(0, SeekOrigin.Begin);
        Assert.True(AwbHeaderReader.TryHashHeader(stream, out var secondHash));
        Assert.Equal(firstHash, secondHash);
    }
}