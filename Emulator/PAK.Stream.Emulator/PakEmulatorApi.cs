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
using PAK.Stream.Emulator.Utilities;

namespace PAK.Stream.Emulator;

/// <summary>
/// Tries to create a PAK emulator.
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
        if (!_pakEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, ref emulated, out var stream))
        {
            _logger.Error("[PakEmulatorApi] TryCreateFromFileSlice: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }

        _logger.Info("[PakEmulatorApi] TryCreateFromFileSlice: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!);
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

    public void AddFile(string file, string route, string inPakPath)
    {
        _pakEmulator.AddFile(file, route, inPakPath);
    }

    public void AddDirectory(string dir)
    {
        _pakEmulator.AddFromFolders(dir);
    }

    public ReadOnlyMemory<byte>? GetEntry(Strim pak, string entryPath)
    {
        entryPath = entryPath.Replace('\\', '/');
        if (!entryPath.StartsWith("/")) 
            entryPath = '/' + entryPath;
        var entry = PakReader.ReadFileFromPak(pak, entryPath, Path.GetPathRoot(entryPath));
        if (entry == null) return null;
        return entry.AsMemory();
    }
}