using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Text.Encodings;
using BF.File.Emulator.Bf;
using FileEmulationFramework.Lib.Memory;
using FileEmulationFramework.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;

namespace FileEmulationFramework.Tests.Emulators.BF;

public class BfEmulatorTests
{
    public BfEmulatorTests()
    {
        // Setup shift_jis encoding for script compiler
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
    }

    [Fact]
    public void SingleFlow()
    {
        var flowFormat = FlowFormatVersion.Version1;
        var library = LibraryLookup.GetLibrary("P4G");
        var encoding = AtlusEncoding.GetByName("P4");

        var builder = new BfBuilder();
        builder.AddFlowFile(Assets.SimpleFlow);
        var handle = Native.CreateFileW(Assets.BaseBf, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        var stream = builder.Build(handle, Assets.BaseBf, flowFormat, library, encoding);

        // Write to file for checking.
        using var fileStream = new FileStream("field.bf", FileMode.Create);
        stream.CopyTo(fileStream);
        fileStream.Close();

        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.SimpleFlowCompiled), File.ReadAllBytes("field.bf"));

    }

    [Fact]
    public void MultipleFlows()
    {
        var flowFormat = FlowFormatVersion.Version1;
        var library = LibraryLookup.GetLibrary("P4G");
        var encoding = AtlusEncoding.GetByName("P4");

        var builder = new BfBuilder();
        builder.AddFlowFile(Assets.SimpleFlow);
        builder.AddFlowFile(Assets.ComplexFlow);
        var handle = Native.CreateFileW(Assets.BaseBf, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        var stream = builder.Build(handle, Assets.BaseBf, flowFormat, library, encoding);

        // Write to file for checking.
        using var fileStream = new FileStream("field.bf", FileMode.Create);
        stream.CopyTo(fileStream);
        fileStream.Close();

        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.MultipleFlowsCompiled), File.ReadAllBytes("field.bf"));
    }

}
