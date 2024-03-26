using BMD.File.Emulator.Interfaces;
using BMD.File.Emulator.Interfaces.Structures.IO;
using BMD.File.Emulator.Utilities;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.Memory;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BMD.File.Emulator;

public class BmdEmulatorApi : IBmdEmulator
{
    private readonly IEmulationFramework _framework;
    private readonly BmdEmulator _bmdEmulator;
    private readonly Logger _logger;

    public BmdEmulatorApi(IEmulationFramework framework, BmdEmulator bmdEmulator, Logger logger)
    {
        _framework = framework;
        _bmdEmulator = bmdEmulator;
        _logger = logger;
    }

    RouteFileTuple[] IBmdEmulator.GetEmulatorInput()
    {
        // Map input to API.
        var input = _bmdEmulator.GetInput();
        var result = GC.AllocateUninitializedArray<RouteFileTuple>(input.Count);
        for (int x = 0; x < result.Length; x++)
        {
            var original = input[x];
            result[x] = new RouteFileTuple()
            {
                Route = original.Route.FullPath,
                File = original.File
            };
        }

        return result;
    }

    public void InvalidateFile(string bmdPath)
    {
        _bmdEmulator.UnregisterFile(bmdPath);
        _framework.UnregisterVirtualFile(bmdPath, false);
    }

    public void RegisterBmd(string sourcePath, string destinationPath)
    {
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[BmdEmulatorApi] RegisterBmd: Failed to open bmd file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return;
        }

        Native.SetFilePointerEx(handle, 0, IntPtr.Zero, 0);

        var fileStream = new FileStream(new SafeFileHandle(handle, true), FileAccess.Read);
        var emulated = new EmulatedFile<FileStream>(fileStream);
        _bmdEmulator.RegisterFile(destinationPath, fileStream);
        _framework.RegisterVirtualFile(destinationPath, emulated, false);

        _logger.Info("[BmdEmulatorApi] Registered bmd {0} at {1}", sourcePath, destinationPath);
    }

    public bool TryCreateFromBmd(string sourcePath, string route, string destinationPath)
    {
        _logger.Info("[BmdEmulatorApi] TryCreateFromBmd: {0}, Route {1}", sourcePath, route);
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[BmdEmulatorApi] TryCreateFromBmd: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, 0, IntPtr.Zero, 0);
        if (!_bmdEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, ref emulated, out var stream))
        {
            _logger.Error("[BmdEmulatorApi] TryCreateFromBmd: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }

        _logger.Info("[BmdEmulatorApi] TryCreateFromBmd: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!, false);
        return true;
    }

    public void AddFile(string file, string route)
    {
        _bmdEmulator.AddFile(file, route);
    }

    public void AddDirectory(string dir)
    {
        _bmdEmulator.AddFromFolders(dir);
    }
}
