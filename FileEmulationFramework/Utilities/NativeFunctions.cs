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
    public IFunction<Native.NtQueryFullAttributesFileFn> NtQueryFullAttributes;
    public IFunction<Native.CloseHandleFn> CloseHandle;

    public NativeFunctions(IntPtr ntCreateFile, IntPtr ntReadFile, IntPtr ntSetInformationFile, IntPtr ntQueryInformationFile, IntPtr ntQueryFullAttributesFile, IntPtr closeHandle, IReloadedHooks hooks)
    {
        NtCreateFile = hooks.CreateFunction<Native.NtCreateFileFn>(ntCreateFile);
        NtReadFile = hooks.CreateFunction<Native.NtReadFileFn>(ntReadFile);
        SetFilePointer = hooks.CreateFunction<Native.NtSetInformationFileFn>(ntSetInformationFile);
        GetFileSize = hooks.CreateFunction<Native.NtQueryInformationFileFn>(ntQueryInformationFile);
        NtQueryFullAttributes = hooks.CreateFunction<Native.NtQueryFullAttributesFileFn>(ntQueryFullAttributesFile);
        CloseHandle = hooks.CreateFunction<Native.CloseHandleFn>(closeHandle);
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
        var ntQueryAttributesPointer = NativeFn.GetProcAddress(ntdllHandle, "NtQueryFullAttributesFile");

        var k32Handle = NativeFn.LoadLibrary("kernel32");
        var closeHandle = NativeFn.GetProcAddress(k32Handle, "CloseHandle");
        
        _instance = new(ntCreateFilePointer, ntReadFilePointer, setFilePointer, getFileSize, ntQueryAttributesPointer, closeHandle, hooks);
        _instanceMade = true;

        return _instance;
    }
}