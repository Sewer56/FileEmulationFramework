using System.Runtime.InteropServices;
using AWB.Stream.Emulator.Interfaces;
using AWB.Stream.Emulator.Interfaces.Structures.IO;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;

namespace AWB.Stream.Emulator;

/// <summary>
/// Tries to create an AWB emulator.
/// </summary>
public class AwbEmulatorApi : IAwbEmulator
{
    private readonly IEmulationFramework _framework;
    private readonly AwbEmulator _awbEmulator;
    private readonly AcbPatcherEmulator _acbEmulator;
    private readonly Logger _logger;

    public AwbEmulatorApi(IEmulationFramework framework, AwbEmulator awbEmulator, AcbPatcherEmulator acbEmulator, Logger logger)
    {
        _framework = framework;
        _awbEmulator = awbEmulator;
        _acbEmulator = acbEmulator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath)
    {
        _logger.Info("[AwbEmulatorApi] TryCreateFromFileSlice: {0}, Ofs {1}, Route {2}", sourcePath, offset, route);
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("[AwbEmulatorApi] TryCreateFromFileSlice: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, offset, IntPtr.Zero, 0);
        if (!_awbEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, false, ref emulated, out var stream))
        {
            _logger.Error("[AwbEmulatorApi] TryCreateFromFileSlice: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }
        
        _logger.Info("[AwbEmulatorApi] TryCreateFromFileSlice: Registering {0}", destinationPath);
        _framework.RegisterVirtualFile(destinationPath, emulated!);
        _awbEmulator.InvokeOnStreamCreated(handle, destinationPath, stream!);
        return true;
    }

    public void InvalidateFile(string awbPath)
    {
        _awbEmulator.UnregisterFile(awbPath);
        _acbEmulator.UnregisterFile(awbPath);
        _framework.UnregisterVirtualFile(awbPath);
    }

    public RouteGroupTuple[] GetEmulatorInput()
    {
        // Map input to API.
        var input = _awbEmulator.GetInput();
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
}