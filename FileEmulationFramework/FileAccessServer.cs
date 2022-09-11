using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Utilities;
using Reloaded.Hooks.Definitions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FileEmulationFramework.Structs;
using Reloaded.Memory.Pointers;
using static FileEmulationFramework.Lib.Utilities.Native;
using static FileEmulationFramework.Utilities.Native;

namespace FileEmulationFramework;

/// <summary>
/// Class responsible for handling of all file access.
/// </summary>
public static unsafe class FileAccessServer
{
    private static Logger _logger = null!;

    private static readonly Dictionary<IntPtr, FileInformation> _handleToInfoMap = new();
    private static readonly object _threadLock = new();
    private static IHook<NtCreateFileFn> _createFileHook = null!;
    private static IHook<NtReadFileFn> _readFileHook = null!;
    private static IHook<NtSetInformationFileFn> _setFilePointerHook = null!;
    private static IHook<NtQueryInformationFileFn> _getFileSizeHook = null!;

    // Extended to 64-bit, to use with NtReadFile
    private const long FILE_USE_FILE_POINTER_POSITION = unchecked((long)0xfffffffffffffffe);

    /// <summary>
    /// Initialises this instance.
    /// </summary>
    public static void Init(Logger logger, NativeFunctions functions)
    {
        _logger = logger;
        _createFileHook = functions.NtCreateFile.Hook(typeof(FileAccessServer), nameof(NtCreateFileImpl)).Activate();
        _readFileHook = functions.NtReadFile.Hook(typeof(FileAccessServer), nameof(NtReadFileImpl)).Activate();
        _setFilePointerHook = functions.SetFilePointer.Hook(typeof(FileAccessServer), nameof(SetInformationFileHook)).Activate();
        _getFileSizeHook = functions.GetFileSize.Hook(typeof(FileAccessServer), nameof(QueryInformationFileImpl)).Activate();
    }

    /// <summary>
    /// Tries to get the information for a file behind a handle.
    /// </summary>
    public static bool TryGetInfoForHandle(IntPtr handle, out FileInformation? info)
    {
        if (!_handleToInfoMap.ContainsKey(handle))
        {
            info = null;
            return false;
        }

        info = _handleToInfoMap[handle];
        return true;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int QueryInformationFileImpl(IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass)
    {
        lock (_threadLock)
        {
            var result = _getFileSizeHook.OriginalFunction.Value.Invoke(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);
            if (fileInformationClass != FileInformationClass.FileStandardInformation || !_handleToInfoMap.TryGetValue(hfile, out var info)) 
                return result;

            var information = (FILE_STANDARD_INFORMATION*)fileInformation;
            var oldSize = information->EndOfFile;
            var newSize = info.Emulator.GetFileSize(hfile);
            if (newSize != -1)
                information->EndOfFile = newSize;

            _logger.Info("File Size Override | Old: {0}, New: {1} | {2}", oldSize, information->EndOfFile, info.FilePath);
            return result;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int SetInformationFileHook(IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass)
    {
        lock (_threadLock)
        {
            return SetInformationFileImpl(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);
        }
    }

    private static int SetInformationFileImpl(IntPtr hfile, IO_STATUS_BLOCK* ioStatusBlock, byte* fileInformation, uint length, FileInformationClass fileInformationClass)
    {
        if (fileInformationClass != FileInformationClass.FilePositionInformation || !_handleToInfoMap.ContainsKey(hfile))
            return _setFilePointerHook.OriginalFunction.Value.Invoke(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);

        var pointer = *(long*)fileInformation;
        _handleToInfoMap[hfile].FilePointer = pointer;
        return _setFilePointerHook.OriginalFunction.Value.Invoke(hfile, ioStatusBlock, fileInformation, length, fileInformationClass);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int NtReadFileImpl(IntPtr handle, IntPtr hEvent, IntPtr* apcRoutine, IntPtr* apcContext, IO_STATUS_BLOCK* ioStatus, byte* buffer, uint length, long* byteOffset, IntPtr key)
    {
        lock (_threadLock)
        {
            // Check if this is one of our files.
            if (!_handleToInfoMap.TryGetValue(handle, out var info))
                return _readFileHook.OriginalFunction.Value.Invoke(handle, hEvent, apcRoutine, apcContext, ioStatus, buffer, length, byteOffset, key);

            // If it is, prepare to hook it.
            long requestedOffset = byteOffset != (void*)0 ? *byteOffset : FILE_USE_FILE_POINTER_POSITION; // -1 means use current location
            if (requestedOffset == FILE_USE_FILE_POINTER_POSITION)
                requestedOffset = _handleToInfoMap[handle].FilePointer;

            if (_logger.IsEnabled(LogSeverity.Debug))
                _logger.Debug($"[AFSHook] Read Request, Buffer: {(long)buffer:X}, Length: {length}, Offset: {requestedOffset}");

            bool result = info.Emulator.ReadData(handle, buffer, length, requestedOffset, out var numReadBytes);
            if (result)
            {
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
        lock (_threadLock)
        {
            string oldFileName = objectAttributes->ObjectName->ToString();
            if (!TryGetFullPath(oldFileName, out var newFilePath))
                return _createFileHook.OriginalFunction.Value.Invoke(handle, access, objectAttributes, ioStatus, allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);

            // Blacklist DLLs to prevent JIT from locking when new assemblies used by this method are loaded.
            // Might want to disable some other extensions in the future; but this is just a temporary bugfix.
            if (newFilePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return _createFileHook.OriginalFunction.Value.Invoke(handle, access, objectAttributes, ioStatus, allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);

            _logger.Debug("[FileAccessServer] Accessing: {0}, {1}", *handle, newFilePath);
            // TODO: Try Accept New File
            // Check if AFS file and register if it is.
            /*
            if (newFilePath.Contains(Constants.AfsExtension, StringComparison.OrdinalIgnoreCase))
            {
                var result = _createFileHook.OriginalFunction.Value.Invoke(handle, access, objectAttributes, ioStatus, allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);
                if (IsAfsFile(newFilePath))
                {
                    _logger.Info($"[FileAccessServer] Opening File Handle: {handle}, File: {newFilePath}");
                    _handleToInfoMap[*handle.Pointer] = new(newFilePath, 0);
                    OnAfsHandleOpened(handle, newFilePath);
                }
                return result;
            }
            */
            var ntStatus = _createFileHook.OriginalFunction.Value.Invoke(handle, access, objectAttributes, ioStatus, allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);

            // Invalidate Duplicate Handles (until we implement NtClose hook).
            if (_handleToInfoMap.Remove(*handle, out var value))
                _logger.Debug("[FileAccessServer] Removed old disposed handle: {0}, File: {1}", *handle, value.FilePath);

            return ntStatus;
        }
    }

    /// <summary>
    /// Tries to resolve a given file path from NtCreateFile to a full file path.
    /// </summary>
    private static bool TryGetFullPath(string oldFilePath, out string newFilePath)
    {
        const string prefix = "\\??\\";
        const int prefixLength = 4;

        if (oldFilePath.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            oldFilePath = oldFilePath.Substring(prefixLength);

        if (!String.IsNullOrEmpty(oldFilePath))
        {
            newFilePath = Path.GetFullPath(oldFilePath);
            return true;
        }

        newFilePath = oldFilePath;
        return false;
    }
}