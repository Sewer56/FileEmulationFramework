using System;
using System.IO;
using AFS.Stream.Emulator.Afs;
using AFSLib;
using FileEmulationFramework.Lib.Utilities;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AFS;

/// <summary>
/// Note: These are for manual review.
/// </summary>
public class AfsEmulatorTests
{
    [Fact]
    public void Replace_SingleFile()
    {
        // Create Builder & Inject Single File
        var builder = new AfsBuilder();
        var handle = Native.CreateFileW(Assets.AfsEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(0, Assets.AssetArgSiren);
        var stream = builder.Build(handle, Assets.AfsEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("output.afs", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = AfsArchive.SeekToAndLoadDataFromIndex(stream, 0);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgSiren), fromStream);
    }

    [Fact]
    public void Extend_File()
    {
        // Create Builder & Inject Single File
        var builder = new AfsBuilder();
        var handle = Native.CreateFileW(Assets.AfsEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(6, Assets.AssetArgSiren);
        var stream = builder.Build(handle, Assets.AfsEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("output.afs", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = AfsArchive.SeekToAndLoadDataFromIndex(stream, 6);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgSiren), fromStream);
    }
}