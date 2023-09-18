using SPD.File.Emulator.Interfaces;
using SPD.File.Emulator.Interfaces.Structures.IO;
using SPD.File.Emulator.Utilities;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.Memory;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SPD.File.Emulator;

public class SpdEmulatorApi : ISpdEmulator
{
    private readonly IEmulationFramework _framework;
    private readonly SpdEmulator _spdEmulator;
    private readonly Logger _logger;

    public SpdEmulatorApi(IEmulationFramework framework, SpdEmulator spdEmulator, Logger logger)
    {
        _framework = framework;
        _spdEmulator = spdEmulator;
        _logger = logger;
    }
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath)
    {
        _logger.Info("[SpdEmulatorApi] TryCreateFromFileSlice: {0}, Ofs {1}, Route {2}", sourcePath, offset, route);
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[SpdEmulatorApi] TryCreateFromFileSlice: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, offset, IntPtr.Zero, 0);
        if (!_spdEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, ref emulated, out var stream))
        {
            _logger.Error("[SpdEmulatorApi] TryCreateFromFileSlice: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }

        _logger.Info("[SpdEmulatorApi] TryCreateFromFileSlice: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!);
        return true;
    }

    public RouteGroupTuple[] GetEmulatorInput()
    {
        // Map input to API.
        var input = _spdEmulator.GetInput();
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

    public void InvalidateFile(string spdPath)
    {
        _spdEmulator.UnregisterFile(spdPath);
        _framework.UnregisterVirtualFile(spdPath);
    }

    public void RegisterSpd(string sourcePath, string destinationPath)
    {
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[SpdEmulatorApi] RegisterSpd: Failed to open spd file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return;
        }

        Native.SetFilePointerEx(handle, 0, IntPtr.Zero, 0);

        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var stream = StreamUtils.CreateMemoryStream(fileStream.Length);
        fileStream.CopyTo(stream);

        var emulated = new EmulatedFile<Stream>(stream);
        _spdEmulator.RegisterFile(destinationPath, stream);
        _framework.RegisterVirtualFile(destinationPath, emulated);

        _logger.Info("[SpdEmulatorApi] Registered spd {0} at {1}", sourcePath, destinationPath);
    }

    public bool TryCreateFromSpd(string sourcePath, string route, string destinationPath)
    {
        _logger.Info("[SpdEmulatorApi] TryCreateFromSpd: {0}, Route {1}", sourcePath, route);
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[SpdEmulatorApi] TryCreateFromSpd: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, 0, IntPtr.Zero, 0);
        if (!_spdEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, ref emulated, out var stream))
        {
            _logger.Error("[SpdEmulatorApi] TryCreateFromSpd: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }

        _logger.Info("[SpdEmulatorApi] TryCreateFromSpd: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!);
        return true;
    }
}
