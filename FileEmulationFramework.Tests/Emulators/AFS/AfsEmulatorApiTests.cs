using System;
using System.IO;
using AFS.Stream.Emulator;
using AFS.Stream.Emulator.Interfaces;
using AFSLib;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AFS;

public class AfsEmulatorApiTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(ushort.MaxValue)]
    public void AddFile_should_replace_requested_index(int index)
    {
        var emulator = new AfsEmulator(null!, false);
        IAfsEmulator api = new AfsEmulatorApi(emulator);
        api.AddFile(Assets.AssetArgSiren, Path.GetFileName(Assets.AfsEmulatorSampleFile), index);

        using var stream = Emulate(emulator);
        var actual = AfsArchive.SeekToAndLoadDataFromIndex(stream, index);

        Assert.Equal(File.ReadAllBytes(Assets.AssetArgSiren), actual);
    }

    [Fact]
    public void AddFile_should_not_emulate_archive_when_route_does_not_match()
    {
        var emulator = new AfsEmulator(null!, false);
        IAfsEmulator api = new AfsEmulatorApi(emulator);
        api.AddFile(Assets.AssetArgSiren, "another.afs", 0);

        var handle = OpenSample();
        try
        {
            Assert.False(emulator.TryCreateFile(handle, Assets.AfsEmulatorSampleFile, "", out _));
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    [Fact]
    public void AddFile_should_not_change_cached_archive_when_registered_after_emulation()
    {
        var emulator = new AfsEmulator(null!, false);
        IAfsEmulator api = new AfsEmulatorApi(emulator);
        var route = Path.GetFileName(Assets.AfsEmulatorSampleFile);
        api.AddFile(Assets.AssetArgSiren, route, 0);

        using var firstStream = Emulate(emulator);
        api.AddFile(Assets.AssetArgHeehoo, route, 0);
        var secondStream = Emulate(emulator);

        Assert.Same(firstStream, secondStream);
        Assert.Equal(File.ReadAllBytes(Assets.AssetArgSiren), AfsArchive.SeekToAndLoadDataFromIndex(secondStream, 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ushort.MaxValue + 1)]
    public void AddFile_should_reject_out_of_range_index_without_registration(int index)
    {
        var emulator = new AfsEmulator(null!, false);
        IAfsEmulator api = new AfsEmulatorApi(emulator);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            api.AddFile(Assets.AssetArgSiren, Path.GetFileName(Assets.AfsEmulatorSampleFile), index));

        var handle = OpenSample();
        try
        {
            Assert.False(emulator.TryCreateFile(handle, Assets.AfsEmulatorSampleFile, "", out _));
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    [Fact]
    public void AddDirectory_should_load_redirector_style_root()
    {
        var root = Path.GetFullPath("Emulators/AFS/api-root");
        var source = Path.Combine(root, Path.GetFileName(Assets.AfsEmulatorSampleFile), "0.bin");
        var emulator = new AfsEmulator(null!, false);
        IAfsEmulator api = new AfsEmulatorApi(emulator);
        api.AddDirectory(root);

        using var stream = Emulate(emulator);
        var actual = AfsArchive.SeekToAndLoadDataFromIndex(stream, 0);
        Assert.Equal(File.ReadAllBytes(source), actual);
    }

    private static MultiStream Emulate(AfsEmulator emulator)
    {
        var handle = OpenSample();
        try
        {
            Assert.True(emulator.TryCreateFile(handle, Assets.AfsEmulatorSampleFile, "", out var emulatedFile));
            return Assert.IsType<EmulatedFile<MultiStream>>(emulatedFile).BaseStream;
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    private static IntPtr OpenSample() => Native.CreateFileW(
        Assets.AfsEmulatorSampleFile,
        FileAccess.Read,
        FileShare.Read,
        IntPtr.Zero,
        FileMode.Open,
        FileAttributes.Normal,
        IntPtr.Zero);
}
