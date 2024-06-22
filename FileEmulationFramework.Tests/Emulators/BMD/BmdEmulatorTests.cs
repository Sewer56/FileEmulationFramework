using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Text.Encodings;
using FileEmulationFramework.Lib.Utilities;
using System;
using System.IO;
using System.Text;
using BMD.File.Emulator.Bmd;
using Xunit;
using MessageFormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;

namespace FileEmulationFramework.Tests.Emulators.BMD;

public class BmdEmulatorTests
{
    public BmdEmulatorTests()
    {
        // Setup shift_jis encoding for script compiler
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public void SingleMsg()
    {
        var msgFormat = MessageFormatVersion.Version1BigEndian;
        var library = LibraryLookup.GetLibrary("P5R");
        var encoding = AtlusEncoding.GetByName("P5R");

        var builder = new BmdBuilder();
        builder.AddMsgFile(Assets.FirstMsg);
        var handle = Native.CreateFileW(Assets.BaseBmd, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        var stream = builder.Build(handle, Assets.BaseBmd, msgFormat, library, encoding);

        // Write to file for checking.
        using var fileStream = new FileStream("e722_103.bmd", FileMode.Create);
        stream.CopyTo(fileStream);
        fileStream.Close();

        // Parse file and check.
        var expected = File.ReadAllBytes(Assets.SingleMsgCompiled);
        var actual = File.ReadAllBytes("e722_103.bmd");
        // TODO: assert failure. this appears to have no effect in actual-ingame use
        Assert.Equal(expected, actual);

    }

    [Fact]
    public void MultipleMsgs()
    {
        var msgFormat = MessageFormatVersion.Version1BigEndian;
        var library = LibraryLookup.GetLibrary("P5R");
        var encoding = AtlusEncoding.GetByName("P5R");

        var builder = new BmdBuilder();
        builder.AddMsgFile(Assets.FirstMsg);
        builder.AddMsgFile(Assets.SecondMsg);
        var handle = Native.CreateFileW(Assets.BaseBmd, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        var stream = builder.Build(handle, Assets.BaseBmd, msgFormat, library, encoding);

        // Write to file for checking.
        using var fileStream = new FileStream("e722_103.bmd", FileMode.Create);
        stream.CopyTo(fileStream);
        fileStream.Close();

        // Parse file and check.
        // TODO: assert failure. this appears to have no effect in actual-ingame use
        Assert.Equal(File.ReadAllBytes(Assets.MultipleMsgsCompiled), File.ReadAllBytes("e722_103.bmd"));
    }

}
