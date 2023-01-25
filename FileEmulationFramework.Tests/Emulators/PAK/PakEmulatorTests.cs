using System;
using System.IO;
using PAK.Stream.Emulator.Pak;
using FileEmulationFramework.Lib.Utilities;
using Xunit;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Streams.Readers;
using Reloaded.Memory.Streams;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
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
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren);
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
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren);
        var stream = builder.Build(handle, Assets.PakV2EmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV2.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }

    [Fact]
    public void Replace_SingleFileV2BE()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV2BEEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren);
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
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren);
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
        builder.AddOrReplaceFile(Assets.AssetArgPAKSiren);
        var stream = builder.Build(handle, Assets.PakV3BEEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputV3BE.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgPAKSiren);
        // Parse file and check.
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgPAKSiren), fromStream);
    }

    [Fact]
    public void Extend_FileV1()
    {
        // Create Builder & Inject Single File
        var builder = new PakBuilder();
        var handle = Native.CreateFileW(Assets.PakV1EmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(Assets.AssetArgMario);
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
        builder.AddOrReplaceFile(Assets.AssetArgMario);
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
        builder.AddOrReplaceFile(Assets.AssetArgMario);
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
        builder.AddOrReplaceFile(Assets.AssetArgMario);
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
        builder.AddOrReplaceFile(Assets.AssetArgMario);
        var stream = builder.Build(handle, Assets.PakV3BEEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("outputEV3BE.pak", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromPak(fileStream, Assets.AssetArgMario);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgMario), fromStream);
    }

    private unsafe byte[] ReadFileFromPak(Stream fileStream, string index)
    {
        var pos = fileStream.Position;
        var filename = Path.GetFileName(index);
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
                fileStream.TryRead(out int NumberOfFiles, out _);
                if (format == FormatVersion.Version3BE || format == FormatVersion.Version2BE)
                    NumberOfFiles = Endian.Reverse(NumberOfFiles);
                for (int i = 0; i < NumberOfFiles; i++)
                {
                    IEntry entry;
                    if(format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
                    {
                        fileStream.TryRead(out V2FileEntry fileentry, out _);
                        entry = fileentry;
                    }
                    else
                    {
                        fileStream.TryRead(out V3FileEntry fileentry, out _);
                        entry = fileentry;
                    }
                    var length = (format == FormatVersion.Version3BE || format == FormatVersion.Version2BE) ? Endian.Reverse(entry.Length) : entry.Length;
                    if (entry.FileName == filename)
                    {
                        var result = GC.AllocateUninitializedArray<byte>(length);
                        fileStream.Read(result, 0, length);
                        return result;
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
                        fileStream.Read(result, 0, fileentry.Length );
                        return result;
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