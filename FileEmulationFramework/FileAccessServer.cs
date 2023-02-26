using System.Diagnostics;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Utilities;
using Reloaded.Hooks.Definitions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static FileEmulationFramework.Lib.Utilities.Native;
using static FileEmulationFramework.Utilities.Native;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib;
using FileEmulationFramework.Structs;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using Native = FileEmulationFramework.Lib.Utilities.Native;
// ReSharper disable RedundantArgumentDefaultValue

namespace FileEmulationFramework;

/// <summary>
/// Class responsible for handling of all file access.
/// </summary>
public static unsafe class FileAccessServer
{
    private static Logger _logger = null!;
    
    /// <summary>
    /// The emulators currently held by the framework.
    /// </summary>
    private static List<IEmulator> Emulators { get; } = new();
    
    private static readonly Dictionary<IntPtr, FileInformation> HandleToInfoMap = new();
    private static readonly Dictionary<string, FileInformation> PathToVirtualFileMap = new(StringComparer.OrdinalIgnoreCase);
    
    private static readonly object ThreadLock = new();
    private static IHook<NtCreateFileFn> _createFileHook = null!;
    private static IHook<NtReadFileFn> _readFileHook = null!;
    private static IHook<NtSetInformationFileFn> _setFilePointerHook = null!;
    private static IHook<NtQueryInformationFileFn> _getFileSizeHook = null!;
    private static IAsmHook _closeHandleHook = null!;
    private static NtQueryInformationFileFn _ntQueryInformationFile;

    private static Route _currentRoute;
    private static nint* _closeHandleThreadId = (nint*)NativeMemory.Alloc((nuint)sizeof(nint));

    // Extended to 64-bit, to use with NtReadFile
    private const long FileUseFilePointerPosition = unchecked((long)0xfffffffffffffffe);

    /// <summary>
    /// Initialises this instance.
    /// </summary>
    public static void Init(Logger logger, NativeFunctions functions, IReloadedHooks? hooks)
    {
        _logger = logger;
        _readFileHook = functions.NtReadFile.Hook(typeof(FileAccessServer), nameof(NtReadFileImpl)).Activate();
        _setFilePointerHook = functions.SetFilePointer.Hook(typeof(FileAccessServer), nameof(SetInformationFileHook)).Activate();
        _getFileSizeHook = functions.GetFileSize.Hook(typeof(FileAccessServer), nameof(QueryInformationFileImpl)).Activate();
        _ntQueryInformationFile = _getFileSizeHook.OriginalFunction;
        
        // We need to cook some assembly for NtClose, because Native->Managed
        // transition can invoke thread setup code which will call CloseHandle again
        // and that will lead to infinite recursion
        var utilities = hooks!.Utilities;
        var getFileTypeAddr = functions.GetFileType.Address;
        var closeHandleCallbackAddr = (long)utilities.GetFunctionPointer(typeof(FileAccessServer), nameof(CloseHandleCallback));
        
        // I verified Wine implements this part of the TEB, Sewer
        const string defaultThreadId = "-1";
        *_closeHandleThreadId = -1;
        
        long threadIdPtr = (long)_closeHandleThreadId;
        if (IntPtr.Size == 4)
        {
            _closeHandleHook = hooks.CreateAsmHook(new[]
            {
                "use32",
                "push esi",
                
                // EAX, ECX, EDX are volatile and free to use per STDCALL convention, rest is backed up
                // EAX: Temporary/Scratch Variable
                // ECX: Saved Thread ID
                // EDX: Thread ID Address
                // ESI: Current Thread ID
                
                // we put called address in EDI
                "mov esi, [fs:18h]",  // get TEB/TIB
                "mov esi, [esi+24h]", // <= thread id in esi
                
                // Check if allow recurse
                $"mov ecx, [{threadIdPtr}]",
                "cmp ecx, esi",
                "je exit",

                // Loop
                "checkInterlocked:",
                    $"lea edx, [{threadIdPtr}]", 
                    $"mov eax, {defaultThreadId}", 
                    "lock cmpxchg [edx], esi", // cmpxchg compares EAX, with ESI [thread id]. See: Interlocked.CompareExchange
                    $"cmp eax, {defaultThreadId}",
                    "jne checkInterlocked",

                // Fire our callback.
                "push dword [esp+8]", // re-push handle
                $"mov ecx, {closeHandleCallbackAddr}",
                $"call ecx",
                $"mov dword [{threadIdPtr}], {defaultThreadId}", // release lock
                    
                // It is a file, call our callback.
                $"exit:",
                "pop esi",
            }, functions.CloseHandle.Address, AsmHookBehaviour.ExecuteFirst);
        }
        else
        {
            _closeHandleHook = hooks.CreateAsmHook(new[]
            {
                "use64",

                // handle in (RCX)
                $"push rdi", // Backup RDI as it's non-volatile.
                $"mov rdi, rcx", // Backup handle for later.

                // We must allocate 'shadow space' for the functions we will call in the future under MSFT x64 ABI
                $"sub rsp, 32",

                // Call function to determine if file.
                $"mov rax, {getFileTypeAddr}",
                $"call rax",

                // Check result, 
                $"cmp eax, 1",
                $"jne exit",

                    // It's a file, let's fire our callback.
                    $"mov rcx, rdi", // pass first parameter.
                    $"mov rax, {closeHandleCallbackAddr}",
                    $"call rax",
                
                $"exit:",
                $"add rsp, 32",
                $"mov rcx, rdi", // must restore parameter, to pass to orig function.
                $"pop rdi", // must restore, non-volatile
            }, functions.CloseHandle.Address, AsmHookBehaviour.ExecuteFirst);
        }
        
        _closeHandleHook.Activate();
        _createFileHook = functions.NtCreateFile.Hook(typeof(FileAccessServer), nameof(NtCreateFileImpl)).Activate();
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static void CloseHandleCallback(IntPtr hfile)
    {
        if (!HandleToInfoMap.Remove(hfile, out var value)) 
            return;
        
        value.File.CloseHandle(hfile, value);
        _logger.Debug("[FileAccessServer] Closed emulated handle: {0}, File: {1}", hfile, value.FilePath);
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int QueryInformationFileImpl(IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass)
    {
        lock (ThreadLock)
        {
            var result = _getFileSizeHook.OriginalFunction.Value.Invoke(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);
            if (fileInformationClass != FileInformationClass.FileStandardInformation || !HandleToInfoMap.TryGetValue(hfile, out var info)) 
                return result;

            var information = (FILE_STANDARD_INFORMATION*)fileInformation;
            var oldSize = information->EndOfFile;
            var newSize = info.File.GetFileSize(hfile, info);
            if (newSize != -1)
                information->EndOfFile = newSize;

            _logger.Info("[FileAccessServer] File Size Override | Old: {0}, New: {1} | {2}", oldSize, newSize, info.FilePath);
            return result;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int SetInformationFileHook(IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass)
    {
        lock (ThreadLock)
        {
            return SetInformationFileImpl(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);
        }
    }

    private static int SetInformationFileImpl(IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass)
    {
        if (fileInformationClass != FileInformationClass.FilePositionInformation || !HandleToInfoMap.ContainsKey(hfile))
            return _setFilePointerHook.OriginalFunction.Value.Invoke(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);

        var pointer = *(long*)fileInformation;
        HandleToInfoMap[hfile].FileOffset = pointer;
        return _setFilePointerHook.OriginalFunction.Value.Invoke(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int NtReadFileImpl(IntPtr handle, IntPtr hEvent, IntPtr* apcRoutine, IntPtr* apcContext, IO_STATUS_BLOCK* ioStatus, byte* buffer, uint length, long* byteOffset, IntPtr key)
    {
        lock (ThreadLock)
        {
            // Check if this is one of our files.
            if (!HandleToInfoMap.TryGetValue(handle, out var info))
                return _readFileHook.OriginalFunction.Value.Invoke(handle, hEvent, apcRoutine, apcContext, ioStatus, buffer, length, byteOffset, key);

            // If it is, prepare to hook it.
            long requestedOffset = byteOffset != (void*)0 ? *byteOffset : FileUseFilePointerPosition; // -1 means use current location
            if (requestedOffset == FileUseFilePointerPosition)
                requestedOffset = HandleToInfoMap[handle].FileOffset;

            if (_logger.IsEnabled(LogSeverity.Debug))
                _logger.Debug($"[FileAccessServer] Read Request, Buffer: {(long)buffer:X}, Length: {length}, Offset: {requestedOffset}");

            bool result = info.File.ReadData(handle, buffer, length, requestedOffset, info, out var numReadBytes);
            if (result)
            {
                _logger.Debug("[FileAccessServer] Read Success, Length: {0}, Offset: {1}", numReadBytes, requestedOffset);
                requestedOffset += numReadBytes;
                SetInformationFileImpl(handle, ioStatus, (byte*)&requestedOffset, sizeof(long), FileInformationClass.FilePositionInformation);

                // Set number of read bytes.
                ioStatus->Status = 0;
                ioStatus->Information = new(numReadBytes);
                return 0;
            }

            return _readFileHook.OriginalFunction.Value.Invoke(handle, hEvent, apcRoutine, apcContext, ioStatus, buffer, length, byteOffset, key);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int NtCreateFileImpl(IntPtr* handle, FileAccess access, OBJECT_ATTRIBUTES* objectAttributes, IO_STATUS_BLOCK* ioStatus, long* allocSize, uint fileAttributes, FileShare share, uint createDisposition, uint createOptions, IntPtr eaBuffer, uint eaLength)
    {
        lock (ThreadLock)
        {
            var currentRoute = _currentRoute;
            try
            {
                // Open the handle.
                var ntStatus = _createFileHook.OriginalFunction.Value.Invoke(handle, access, objectAttributes, ioStatus, allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);

                // We get the file path by asking the OS; as to not clash with redirector.
                var hndl = *handle;
                if (!FilePathResolver.TryGetFinalPathName(hndl, out var newFilePath))
                {
                    _logger.Debug("[FileAccessServer] Can't get final file name.");
                    return ntStatus;
                }

                // Blacklist DLLs to prevent JIT from locking when new assemblies used by this method are loaded.
                // Might want to disable some other extensions in the future; but this is just a temporary bugfix.
                if (newFilePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    return ntStatus;

                // We don't support directory emulation, yet.
                if (IsDirectory(hndl))
                    return ntStatus;
                
                // Append to route.
                if (_currentRoute.IsEmpty())
                    _currentRoute = new Route(newFilePath);
                else
                    _currentRoute = _currentRoute.Merge(newFilePath);

                _logger.Debug("[FileAccessServer] Accessing: {0}, {1}, Route: {2}", hndl, newFilePath, _currentRoute.FullPath);

                // Try Accept New File (virtual override)
                if (PathToVirtualFileMap.TryGetValue(newFilePath, out var fileInfo))
                {
                    // Reuse of emulated file (backed by stream) is safe because file access is single threaded.
                    HandleToInfoMap[hndl] = new(fileInfo.FilePath, 0, fileInfo.File);
                    return ntStatus;
                }

                // Try accept new file (emulator)
                for (var x = 0; x < Emulators.Count; x++)
                {
                    var emulator = Emulators[x];
                    if (!emulator.TryCreateFile(hndl, newFilePath, currentRoute.FullPath, out var emulatedFile))
                        continue;

                    HandleToInfoMap[hndl] = new(newFilePath, 0, emulatedFile);
                    return ntStatus;
                }
                
                return ntStatus;
            }
            finally
            {
                _currentRoute = currentRoute;
            }
        }
    }

    /// <summary>
    /// Determines if the given handle refers to a directory.
    /// </summary>
    private static bool IsDirectory(IntPtr hndl)
    {
        // We could use Kernel32 API or C# API itself, but calling deepmost API directly is more efficient. 
        // IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass
        var statusBlock = new IO_STATUS_BLOCK();
        var fileInfo = new FILE_STANDARD_INFORMATION();
        _ntQueryInformationFile.Value.Invoke(hndl, &statusBlock, (byte*)&fileInfo, (uint)sizeof(FILE_STANDARD_INFORMATION), FileInformationClass.FileStandardInformation);
        return fileInfo.Directory;
    }
    
    // PUBLIC API
    internal static void AddEmulator(IEmulator emulator) => Emulators.Add(emulator);
    internal static void RegisterVirtualFile(string filePath, IEmulatedFile file)
    {
        var info = new FileInformation(filePath, 0, file);
        
        // Create dummy file
        Native.CloseHandle(Native.CreateFileW(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Create, FileAttributes.Normal, IntPtr.Zero));
        PathToVirtualFileMap[filePath] = info;
    }

    public static void UnregisterVirtualFile(string filePath)
    {
        PathToVirtualFileMap.Remove(filePath);
        try { File.Delete(filePath); }
        catch (Exception) { /* Ignored */ }
    }
}