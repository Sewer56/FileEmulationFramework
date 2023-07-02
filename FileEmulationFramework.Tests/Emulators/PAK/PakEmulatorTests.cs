using System;
using System.IO;
using PAK.Stream.Emulator.Pak;
using FileEmulationFramework.Lib.Utilities;
using Xunit;
using static PAK.Stream.Emulator.Utilities.PakReader;

namespace FileEmulationFramework.Tests.Emulators.PAK;

/// <summary>
/// Note: These are for manual review.
/// </summary>
public class PakEmulatorTests
{
    [Fact]
    public void Replace_SingleFileV1()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV1EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren, Path.GetDirectoryName(Assets.AssetArgPAKSiren)!);
        var stream = builder.Build(handle, Assets.PakV1EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV1.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }

    [Fact]
    public void Replace_SingleFileV2()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren, Path.GetDirectoryName(Assets.AssetArgPAKSiren));
        var stream = builder.Build(handle, Assets.PakV2EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV2.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }

    [Fact]
    public void Replace_SingleFileV2NESTED()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2NESTEDEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetNestedArgSiren, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV2NESTEDEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV2NESTED.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetNestedArgSiren, Assets.AssetsSoundFolder);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetNestedArgSiren), fromStream);
    }

    [Fact]
    public void Extend_SingleFileV2NESTED()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2NESTEDEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetNestedArgHeehoo, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV2NESTEDEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV2NESTED.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetNestedArgHeehoo, Assets.AssetsSoundFolder);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetNestedArgHeehoo), fromStream);
    }
    [Fact]
    public void Replace_SingleFileV2BE()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2BEEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV2BEEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV2BE.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }
    [Fact]
    public void Replace_SingleFileV3()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV3EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV3EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV3.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }
    [Fact]
    public void Replace_SingleFileV3BE()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV3BEEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV3BEEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV3BE.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }
    [Fact]
    public void Replace_SingleFileinit_free()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.Pakinit_free, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetMiniTV, Assets.AssetsFolder + "/Textures/init_free.bin");
        var stream = builder.Build(handle, Assets.Pakinit_free);

        // Write to file for checking.
        using var fileStream = new FileStream("outputinit_free.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetMiniTV, Assets.AssetsFolder + "/Textures/init_free.bin");
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetMiniTV), fromStream);
    }
    [Fact]
    public void Extend_FileV1()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV1EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgMario, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV1EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV1.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgMario);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgMario), fromStream);
    }

    [Fact]
    public void Extend_FileV2()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgMario, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV2EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV2.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgMario);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgMario), fromStream);
    }

    [Fact]
    public void Extend_FileV2BE()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2BEEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgMario, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV2BEEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV2BE.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgMario);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgMario), fromStream);
    }

    [Fact]
    public void Extend_FileV3()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV3EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgMario, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV3EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV3.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgMario);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgMario), fromStream);
    }

    [Fact]
    public void Extend_FileV3BE()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV3BEEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgMario, Assets.AssetsSoundFolder);
        var stream = builder.Build(handle, Assets.PakV3BEEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV3BE.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgMario);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgMario), fromStream);
    }
}