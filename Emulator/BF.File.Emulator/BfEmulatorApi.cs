using BF.File.Emulator.Interfaces;
using BF.File.Emulator.Interfaces.Structures.IO;
using BF.File.Emulator.Utilities;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace BF.File.Emulator;

public class BfEmulatorApi : IBfEmulator
{
    private readonly IEmulationFramework _framework;
    private readonly BfEmulator _bfEmulator;
    private readonly Logger _logger;

    public BfEmulatorApi(IEmulationFramework framework, BfEmulator bfEmulator, Logger logger)
    {
        _framework = framework;
        _bfEmulator = bfEmulator;
        _logger = logger;
    }

    RouteFileTuple[] IBfEmulator.GetEmulatorInput()
    {
        // Map input to API.
        var input = _bfEmulator.GetInput();
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

    public void InvalidateFile(string bfPath)
    {
        _bfEmulator.UnregisterFile(bfPath);
        _framework.UnregisterVirtualFile(bfPath, false);
    }

    public void RegisterBf(string sourcePath, string destinationPath)
    {
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[BfEmulatorApi] RegisterBf: Failed to open bf file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return;
        }

        Native.SetFilePointerEx(handle, 0, IntPtr.Zero, 0);

        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var stream = StreamUtils.CreateMemoryStream(fileStream.Length);
        fileStream.CopyTo(stream);

        var emulated = new EmulatedFile<Stream>(stream);
        _bfEmulator.RegisterFile(destinationPath, stream);
        _framework.RegisterVirtualFile(destinationPath, emulated, false);

        _logger.Info("[BfEmulatorApi] Registered bf {0} at {1}", sourcePath, destinationPath);
    }

    public bool TryCreateFromBf(string sourcePath, string route, string destinationPath)
    {
        _logger.Info("[BfEmulatorApi] TryCreateFromBf: {0}, Route {1}", sourcePath, route);
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[BfEmulatorApi] TryCreateFromBf: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, 0, IntPtr.Zero, 0);
        if (!_bfEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, ref emulated, out var stream))
        {
            _logger.Error("[BfEmulatorApi] TryCreateFromBf: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }

        _logger.Info("[BfEmulatorApi] TryCreateFromBf: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!, false);
        return true;
    }

    public void AddFile(string file, string route)
    {
        _bfEmulator.AddFile(file, route);
    }

    public void AddDirectory(string dir)
    {
        _bfEmulator.AddFromFolders(dir);
    }

}
