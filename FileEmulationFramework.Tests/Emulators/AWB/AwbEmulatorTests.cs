using System;
using System.IO;
using AWB.Stream.Emulator.Awb;
using AWB.Stream.Emulator.Awb.Structs;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.Streams;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AWB;

/// <summary>
/// Tests for the AWB emulator.
/// </summary>
public class AwbEmulatorTests
{
    [Fact]
    public void Replace_SingleFile()
    {
        // Create Builder & Inject Single File
        var builder = new AwbBuilder();
        var handle = Native.CreateFileW(Assets.AwbEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(0, Assets.AssetArgSiren);
        var stream = builder.Build(handle, Assets.AwbEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("output.awb", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromAwb(fileStream, 0);
        var original = File.ReadAllBytes(Assets.AssetArgSiren);
        Assert.Equal(original, fromStream.AsSpan(0, original.Length).ToArray()); // trim padding
    }

    [Fact]
    public void Extend_File()
    {
        // Create Builder & Inject Single File
        var builder = new AwbBuilder();
        var handle = Native.CreateFileW(Assets.AwbEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        builder.AddOrReplaceFile(6, Assets.AssetArgSiren);
        var stream = builder.Build(handle, Assets.AwbEmulatorSampleFile);

        // Write to file for checking.
        using var fileStream = new FileStream("output.awb", FileMode.Create);
        stream.CopyTo(fileStream);

        // Parse file and check.
        var fromStream = ReadFileFromAwb(fileStream, 6);
        var original = File.ReadAllBytes(Assets.AssetArgSiren);
        Assert.Equal(original, fromStream.AsSpan(0, original.Length).ToArray());
    }

    private unsafe byte[] ReadFileFromAwb(Stream fileStream, int index)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        AwbHeaderReader.TryReadHeader(fileStream, out var header);
        fixed (byte* headerPtr = &header![0])
        {
            var viewer = AwbViewer.FromMemory(headerPtr);
            Span<FileEntry> entries = stackalloc FileEntry[viewer.FileCount];
            viewer.GetEntries(entries);
            
            var entry = entries[index];
            var result = GC.AllocateUninitializedArray<byte>((int)entry.Length);
            fileStream.Seek(entry.Position, SeekOrigin.Begin);
            fileStream.TryReadSafe(result);
            return result;
        }
    }
}