using System;
using System.IO;
using PAK.Stream.Emulator.Pak;
using FileEmulationFramework.Lib.Utilities;
using Xunit;
using Reloaded.Memory;

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

    private byte[] ReadFileFromPak(Stream fileStream, string index, string fileRoot = null)
    {
        var pos = fileStream.Position;
        string filename;
        string container = "";
        if (fileRoot == null)
            filename = Path.GetFileName(index);
        else
        {
            filename = Path.GetRelativePath(fileRoot, index).Replace("\\", "/");
            container = Path.GetDirectoryName(filename)!.Replace("\\", "/");
        }
        fileStream.Seek(0, SeekOrigin.Begin);
        var format = PakBuilder.DetectVersion(fileStream);

        if (format == FormatVersion.Unknown)
        {
            ThrowHelpers.IO("Unknown type of PAK file");
        }

        try
        {
            if (format != FormatVersion.Version1)
            {
                fileStream.TryRead(out int numberOfFiles, out _);
                if (format == FormatVersion.Version3BE || format == FormatVersion.Version2BE)
                    numberOfFiles = Endian.Reverse(numberOfFiles);
                
                for (int i = 0; i < numberOfFiles; i++)
                {
                    IEntry entry;
                    if(format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
                    {
                        fileStream.TryRead(out V2FileEntry fileEntry, out _);
                        entry = fileEntry;
                    }
                    else
                    {
                        fileStream.TryRead(out V3FileEntry fileEntry, out _);
                        entry = fileEntry;
                    }
                    var length = (format == FormatVersion.Version3BE || format == FormatVersion.Version2BE) ? Endian.Reverse(entry.Length) : entry.Length;
                    if (entry.FileName == filename)
                    {
                        var result = GC.AllocateUninitializedArray<byte>(length);
                        fileStream.ReadAtLeast(result, length);
                        return result;
                    }
                    else if(entry.FileName == container)
                    {
                        var result = GC.AllocateUninitializedArray<byte>(length);
                        fileStream.ReadAtLeast(result, length);
                        var file = new MemoryStream(result);
                        return ReadFileFromPak(file, filename, container);
                    }

                    fileStream.Seek(length, SeekOrigin.Current);
                }
                return null;
                
            }
            else
            {
                int i = 0;
                while (i < 1024 )
                {
                    fileStream.TryRead(out V1FileEntry fileentry, out _);
                    if (fileentry.FileName == filename) 
                    {
                        var result = GC.AllocateUninitializedArray<byte>(fileentry.Length);
                        fileStream.ReadAtLeast(result, fileentry.Length);
                        return result;
                    }
                    else if (fileentry.FileName == container)
                    {
                        var result = GC.AllocateUninitializedArray<byte>(fileentry.Length);
                        fileStream.ReadAtLeast(result, fileentry.Length);
                        var file = new MemoryStream(result);
                        return ReadFileFromPak(file, filename, container);
                    }

                    fileStream.Seek(PakBuilder.Align(fileentry.Length, 64), SeekOrigin.Current);
                    if (fileStream.Length < fileStream.Position + 320)
                        return null;
                    i++;
                    
                }
                return null;
            }

        }
        finally
        {
            fileStream.Position = pos;
        }
    }
    
}