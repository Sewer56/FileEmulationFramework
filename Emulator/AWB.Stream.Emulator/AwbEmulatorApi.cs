using System.Runtime.InteropServices;
using AWB.Stream.Emulator.Interfaces;
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
    private readonly Logger _logger;

    public AwbEmulatorApi(IEmulationFramework framework, AwbEmulator awbEmulator, Logger logger)
    {
        _framework = framework;
        _awbEmulator = awbEmulator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath)
    {
        var handle = Native.CreateFileW(sourcePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (handle == new IntPtr(-1))
        {
            _logger.Error("TryCreateFromFileSlice: Failed to open base file with Win32 Error: {0}, Path {1}", Marshal.GetLastWin32Error(), sourcePath);
            return false;
        }

        IEmulatedFile? emulated = null;
        Native.SetFilePointerEx(handle, offset, IntPtr.Zero, 0);
        if (!_awbEmulator.TryCreateEmulatedFile(handle, sourcePath, destinationPath, route, ref emulated))
        {
            _logger.Error("TryCreateFromFileSlice: Failed to Create Emulated File at Path {0}", sourcePath);
            return false;
        }
        
        _framework.RegisterVirtualFile(destinationPath, emulated);
        return true;
    }
}