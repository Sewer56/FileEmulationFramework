using Reloaded.Hooks.Definitions;
using NativeFn = FileEmulationFramework.Lib.Utilities.Native;

namespace FileEmulationFramework.Utilities;

/// <summary>
/// Contains the pointers to the methods we will hook.
/// </summary>
public struct NativeFunctions
{
    private static bool _instanceMade;
    private static NativeFunctions _instance;

    public IFunction<Native.NtCreateFileFn> NtCreateFile;
    public IFunction<Native.NtReadFileFn> NtReadFile;
    public IFunction<Native.NtSetInformationFileFn> SetFilePointer;
    public IFunction<Native.NtQueryInformationFileFn> GetFileSize;
    public IFunction<Native.CloseHandleFn> CloseHandle;
    public IFunction<Native.GetFileTypeFn> GetFileType;

    public NativeFunctions(IntPtr ntCreateFile, IntPtr ntReadFile, IntPtr ntSetInformationFile, IntPtr ntQueryInformationFile, IntPtr closeHandle, IntPtr getFileType, IReloadedHooks hooks)
    {
        NtCreateFile = hooks.CreateFunction<Native.NtCreateFileFn>((long)ntCreateFile);
        NtReadFile = hooks.CreateFunction<Native.NtReadFileFn>((long)ntReadFile);
        SetFilePointer = hooks.CreateFunction<Native.NtSetInformationFileFn>((long)ntSetInformationFile);
        GetFileSize = hooks.CreateFunction<Native.NtQueryInformationFileFn>((long)ntQueryInformationFile);
        CloseHandle = hooks.CreateFunction<Native.CloseHandleFn>((long)closeHandle);
        GetFileType = hooks.CreateFunction<Native.GetFileTypeFn>((long)getFileType);
    }

    /// <summary>
    /// Gets the instance of native struct pointers.
    /// </summary>
    public static NativeFunctions GetInstance(IReloadedHooks hooks)
    {
        if (_instanceMade)
            return _instance;

        var ntdllHandle = NativeFn.LoadLibrary("ntdll");
        var ntCreateFilePointer = NativeFn.GetProcAddress(ntdllHandle, "NtCreateFile");
        var ntReadFilePointer = NativeFn.GetProcAddress(ntdllHandle, "NtReadFile");
        var setFilePointer = NativeFn.GetProcAddress(ntdllHandle, "NtSetInformationFile");
        var getFileSize = NativeFn.GetProcAddress(ntdllHandle, "NtQueryInformationFile");

        var k32Handle = NativeFn.LoadLibrary("kernel32");
        var closeHandle = NativeFn.GetProcAddress(k32Handle, "CloseHandle");
        var getFileType = NativeFn.GetProcAddress(k32Handle, "GetFileType");
        
        _instance = new(ntCreateFilePointer, ntReadFilePointer, setFilePointer, getFileSize, closeHandle, getFileType, hooks);
        _instanceMade = true;

        return _instance;
    }
}