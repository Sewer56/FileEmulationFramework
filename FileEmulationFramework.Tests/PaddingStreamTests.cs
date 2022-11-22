using FileEmulationFramework.Lib.IO;
using Xunit;

namespace FileEmulationFramework.Tests;

/// <summary>
/// Tests related to the stream used for padding.
/// </summary>
public class PaddingStreamTests
{
    [Fact]
    public void PaddingStream_RespectsMaxLength()
    {
        using var paddingStream = new PaddingStream(69, 31);
        var data = new byte[32];

        Assert.Equal(31, paddingStream.Read(data));
        for (int x = 0; x < 31; x++)
            Assert.Equal(69, data[x]);
        
        Assert.Equal(0, data[31]);
    }
}