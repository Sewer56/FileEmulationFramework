using System;
using System.IO;
using System.Linq;
using csharp_prs;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Heroes.SDK;
using Heroes.SDK.Definitions.Structures.Archive.OneFile.Custom;
using Heroes.SDK.Parsers;
using ONE.Heroes.Stream.Emulator;
using ONE.Heroes.Stream.Emulator.Interfaces;
using ONE.Heroes.Stream.Emulator.One;
using Reloaded.Memory.Streams;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.ONE;

public class OneEmulatorApiTests
{
    public OneEmulatorApiTests()
    {
        CompressedFilesCache.Clear();
        CompressedFilesCache.Init(new PrsInstance());
        SDK.Init(null, new PrsInstance());
    }

    [Fact]
    public void AddFile_should_replace_entry_using_lazy_compression()
    {
        using var temp = new TemporaryDirectory();
        var source = temp.Copy(Assets.AssetArgPAKSiren, "arg_mario.flac");
        var emulator = CreateEmulator();
        IOneEmulator api = new OneEmulatorApi(emulator);

        api.AddFile(source, Path.GetFileName(Assets.OneEmulatorSampleFile));
        Assert.False(CompressedFilesCache.TryGetExistingFile(source, out _));

        var archive = Emulate(emulator);
        Assert.True(CompressedFilesCache.TryGetExistingFile(source, out _));
        Assert.Equal(File.ReadAllBytes(source), GetFile(archive, "arg_mario.flac").GetUncompressedData());
    }

    [Fact]
    public void AddFile_should_use_precompressed_input_without_recompressing()
    {
        using var temp = new TemporaryDirectory();
        var source = temp.Copy(Assets.AssetArgPAKSiren, "source.flac");
        var precompressed = temp.PathFor("arg_mario.flac.prs");
        File.WriteAllBytes(precompressed, new PrsInstance().Compress(File.ReadAllBytes(source), 255));

        var emulator = CreateEmulator();
        IOneEmulator api = new OneEmulatorApi(emulator);
        api.AddFile(precompressed, Path.GetFileName(Assets.OneEmulatorSampleFile));

        var archive = Emulate(emulator);
        Assert.False(CompressedFilesCache.TryGetExistingFile(precompressed, out _));
        Assert.Equal(File.ReadAllBytes(source), GetFile(archive, "arg_mario.flac").GetUncompressedData());
    }

    [Fact]
    public void AddDirectory_should_load_redirector_style_root()
    {
        using var temp = new TemporaryDirectory();
        var archiveDirectory = Directory.CreateDirectory(temp.PathFor(Path.GetFileName(Assets.OneEmulatorSampleFile)));
        var source = Path.Combine(archiveDirectory.FullName, "arg_mario.flac");
        File.Copy(Assets.AssetArgPAKSiren, source);
        var emulator = CreateEmulator();
        IOneEmulator api = new OneEmulatorApi(emulator);
        api.AddDirectory(temp.Path);

        var archive = Emulate(emulator);
        Assert.Equal(File.ReadAllBytes(source), GetFile(archive, "arg_mario.flac").GetUncompressedData());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Delete_marker_should_remove_file_name_entry(bool useDirectory)
    {
        using var temp = new TemporaryDirectory();
        var root = useDirectory
            ? Directory.CreateDirectory(temp.PathFor(Path.GetFileName(Assets.OneEmulatorSampleFile))).FullName
            : Directory.CreateDirectory(temp.PathFor("nested")).FullName;
        var marker = Path.Combine(root, "arg_mario.flac.del");
        File.WriteAllBytes(marker, Array.Empty<byte>());
        var emulator = CreateEmulator();
        IOneEmulator api = new OneEmulatorApi(emulator);

        if (useDirectory)
            api.AddDirectory(temp.Path);
        else
            api.AddFile(marker, Path.GetFileName(Assets.OneEmulatorSampleFile));

        var archive = Emulate(emulator);
        Assert.DoesNotContain(archive.GetFiles(), file => file.Name == "arg_mario.flac");
    }

    [Fact]
    public void AddFile_should_preserve_case_sensitive_entry_matching()
    {
        using var temp = new TemporaryDirectory();
        var source = temp.Copy(Assets.AssetArgPAKSiren, "ARG_MARIO.FLAC");
        var emulator = CreateEmulator();
        IOneEmulator api = new OneEmulatorApi(emulator);
        api.AddFile(source, Path.GetFileName(Assets.OneEmulatorSampleFile));

        var archive = Emulate(emulator);
        Assert.Contains(archive.GetFiles(), file => file.Name == "arg_mario.flac");
        Assert.Contains(archive.GetFiles(), file => file.Name == "ARG_MARIO.FLAC");
    }

    private static OneEmulator CreateEmulator() => new(null!);

    private static OneArchive Emulate(OneEmulator emulator)
    {
        var handle = Native.CreateFileW(
            Assets.OneEmulatorSampleFile,
            FileAccess.Read,
            FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            FileAttributes.Normal,
            IntPtr.Zero);

        try
        {
            Assert.True(emulator.TryCreateFile(handle, Assets.OneEmulatorSampleFile, "", out var emulatedFile));
            using var stream = Assert.IsType<EmulatedFile<MultiStream>>(emulatedFile).BaseStream;
            var data = new byte[stream.Length];
            stream.ReadExactly(data);
            return new OneArchive(data);
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    private static ManagedOneFile GetFile(OneArchive archive, string name) =>
        Assert.Single(archive.GetFiles().Where(file => file.Name == name));

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"one-api-{Guid.NewGuid():N}");

        public TemporaryDirectory() => Directory.CreateDirectory(Path);

        public string PathFor(string name) => System.IO.Path.Combine(Path, name);

        public string Copy(string source, string name)
        {
            var destination = PathFor(name);
            File.Copy(source, destination);
            return destination;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch (IOException)
            {
                // Pre-compressed inputs remain open for the lifetime of the emulated archive.
            }
        }
    }
}
