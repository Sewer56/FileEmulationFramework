using System;
using System.IO;
using System.Linq;
using csharp_prs;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Heroes.SDK;
using Heroes.SDK.Parsers;
using ONE.Heroes.Stream.Emulator.One;
using Reloaded.Memory.Streams;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.ONE;

public class OneEmulatorTests
{
    public OneEmulatorTests()
    {
        CompressedFilesCache.Init(new PrsInstance());
        SDK.Init(null, new PrsInstance());
    }

    [Fact]
    public void File_IsCached()
    {
        // Make sure test is valid.
        CompressedFilesCache.Clear();
        Assert.False(CompressedFilesCache.TryGetExistingFile(Assets.AssetArgHeehoo, out _));
        
        // Try cache item.
        var first = CompressedFilesCache.GetFile(Assets.AssetArgHeehoo);
        Assert.True(CompressedFilesCache.TryGetExistingFile(Assets.AssetArgHeehoo, out _));
        var second = CompressedFilesCache.GetFile(Assets.AssetArgHeehoo);
        Assert.Equal(second, first);
    }

    [Fact]
    public void Replace_SingleFile()
    {
        const string fileName = "arg_mario.flac";

        // Make sure test is valid.
        var originalArchive = new OneArchive(File.ReadAllBytes(Assets.OneEmulatorSampleFile));
        var oldFile = originalArchive.GetFiles().FirstOrDefault(file => file.Name == fileName);
        Assert.NotNull(oldFile);

        // Arrange
        var oneBuilder = new OneBuilder();
        var replacedFile = new OneBuilderItem(new FileSliceStreamW32(new FileSlice(Assets.AssetArgHeehoo)), fileName);
        oneBuilder.AddInputFile(replacedFile);

        // Act
        var handle = Native.CreateFileW(Assets.OneEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        var finalStream = oneBuilder.Build(handle, Assets.OneEmulatorSampleFile);

        var newArchiveData = new byte[finalStream.Length];
        finalStream.TryReadSafe(newArchiveData);
        var newArchive = new OneArchive(newArchiveData);
        var newFile = newArchive.GetFiles().FirstOrDefault(file => file.Name == fileName);

        Assert.NotEqual(oldFile.GetUncompressedData().Length, newFile.GetUncompressedData().Length);
    }

    [Fact]
    public void Remove_SingleFile()
    {
        const string fileName = "arg_mario.flac";

        // Make sure test is valid.
        var originalArchive = new OneArchive(File.ReadAllBytes(Assets.OneEmulatorSampleFile));
        var oldFile = originalArchive.GetFiles().FirstOrDefault(file => file.Name == fileName);
        Assert.NotNull(oldFile);

        // Arrange
        var oneBuilder = new OneBuilder();
        oneBuilder.AddDeleteFile(fileName);

        // Act
        var handle = Native.CreateFileW(Assets.OneEmulatorSampleFile, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        var finalStream = oneBuilder.Build(handle, Assets.OneEmulatorSampleFile);

        var newArchiveData = new byte[finalStream.Length];
        finalStream.TryReadSafe(newArchiveData);
        var newArchive = new OneArchive(newArchiveData);
        var newFile = newArchive.GetFiles().FirstOrDefault(file => file.Name == fileName);

        Assert.Null(newFile);
    }
}