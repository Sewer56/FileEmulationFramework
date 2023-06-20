using System.Runtime.InteropServices;
using PAK.Stream.Emulator.Interfaces;
using PAK.Stream.Emulator.Interfaces.Structures.IO;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Lib.IO;
using PAK.Stream.Emulator.Pak;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;
using RouteGroupTuple = PAK.Stream.Emulator.Interfaces.Structures.IO.RouteGroupTuple;
using DirectoryFilesGroup = PAK.Stream.Emulator.Interfaces.Structures.IO.DirectoryFilesGroup;
using DirectoryInformation = PAK.Stream.Emulator.Interfaces.Structures.IO.DirectoryInformation;
using System.Diagnostics;
using Reloaded.Memory;

namespace PAK.Stream.Emulator;

/// <summary>
/// Tries to create an PAK emulator.
/// </summary>
public class PakEmulatorApi : IPakEmulator
{
    private readonly IEmulationFramework _framework;
    private readonly PakEmulator _pakEmulator;
    private readonly Logger _logger;

    public PakEmulatorApi(IEmulationFramework framework, PakEmulator pakEmulator, Logger logger)
    {
        _framework = framework;
        _pakEmulator = pakEmulator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath)
    {
        _logger.Info("[PakEmulatorApi] TryCreateFromFileSlice: {0}, Ofs {1}, Route {2}", sourcePath, offset, route);
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[PakEmulatorApi] TryCreateFromFileSlice: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, offset, IntPtr.Zero, 0);
        if (!_pakEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, false, ref emulated, out var stream))
        {
            _logger.Error("[PakEmulatorApi] TryCreateFromFileSlice: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }

        _logger.Info("[PakEmulatorApi] TryCreateFromFileSlice: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!);
        _pakEmulator.InvokeOnStreamCreated(handle, destinationPath, stream!);
        return true;
    }

    public void InvalidateFile(string pakPath)
    {
        _pakEmulator.UnregisterFile(pakPath);
        _framework.UnregisterVirtualFile(pakPath);
    }

    public RouteGroupTuple[] GetEmulatorInput()
    {
        // Map input to API.
        var input = _pakEmulator.GetInput();
        var result = GC.AllocateUninitializedArray<RouteGroupTuple>(input.Count);
        for (int x = 0; x < result.Length; x++)
        {
            var original = input[x];
            result[x] = new RouteGroupTuple()
            {
                Route = original.Route.FullPath,
                Files = new DirectoryFilesGroup()
                {
                    Files = original.Files.Files,
                    Directory = new DirectoryInformation()
                    {
                        FullPath = original.Files.Directory.FullPath,
                        LastWriteTime = original.Files.Directory.LastWriteTime
                    }
                }
            };
        }

        return result;
    }

    public ReadOnlyMemory<byte>? GetEntry(Strim pak, string entryPath)
    {
        var entry = ReadFileFromPak(pak, entryPath);
        if (entry == null) return null;
        return entry.AsMemory();
    }

    private byte[]? ReadFileFromPak(Strim fileStream, string index, string fileRoot = null)
    {
        var pos = fileStream.Position;
        string filename;
        string container = null;
        if (string.IsNullOrEmpty(fileRoot))
        {
            if (Path.GetDirectoryName(index).Contains("."))
            {
                filename = Path.GetFileName(index);
                container = Path.GetDirectoryName(index)!.Replace("\\", "/");
            }
            else
                filename = index.Replace("\\", "/");

        }
        else
        {
            filename = Path.GetRelativePath(fileRoot, index).Replace("\\", "/");
            container = Path.GetDirectoryName(filename)!.Replace("\\", "/");
        }
        if (container == "") container = null;
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
                    if (format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
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
                    else if (entry.FileName == container)
                    {
                        var result = GC.AllocateUninitializedArray<byte>(length);
                        fileStream.ReadAtLeast(result, length);
                        var file = new MemoryStream(result);
                        return ReadFileFromPak(file, index, container);
                    }

                    fileStream.Seek(length, SeekOrigin.Current);
                }
                return null;

            }
            else
            {
                int i = 0;
                while (i < 1024)
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
                        return ReadFileFromPak(file, index.Replace("\\", "/"), container);
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