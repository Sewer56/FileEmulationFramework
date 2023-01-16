// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static FileEmulationFramework.Lib.Utilities.Native;

#pragma warning disable CS1591

namespace FileEmulationFramework.Lib.IO;

/// <summary>
/// Class that provides WinAPI based utility methods for fast file enumeration in directories.
/// </summary>
[SupportedOSPlatform("windows5.1.2600")]
public class WindowsDirectorySearcher
{
    private const string Prefix = "\\??\\";

    static unsafe delegate* unmanaged[Stdcall]<ref IntPtr, int, ref OBJECT_ATTRIBUTES, ref IO_STATUS_BLOCK, ref long, uint, FileShare, int, uint, IntPtr, uint, IntPtr> NtCreateFilePtr;
    static unsafe delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, ref IO_STATUS_BLOCK, IntPtr, uint, uint, int, IntPtr, int, uint> NtQueryDirectoryFilePtr;
    static unsafe delegate* unmanaged[Stdcall]<IntPtr, int> NtClosePtr;

    static unsafe WindowsDirectorySearcher()
    {
        var ntdll = LoadLibrary("ntdll");
        NtCreateFilePtr = (delegate* unmanaged[Stdcall]<ref IntPtr, int, ref OBJECT_ATTRIBUTES, ref IO_STATUS_BLOCK, ref long, uint, FileShare, int, uint, IntPtr, uint, IntPtr>)GetProcAddress(ntdll, "NtCreateFile");
        NtQueryDirectoryFilePtr = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, ref IO_STATUS_BLOCK, IntPtr, uint, uint, int, IntPtr, int, uint>)GetProcAddress(ntdll, "NtQueryDirectoryFile");
        NtClosePtr = (delegate* unmanaged[Stdcall]<IntPtr, int>)GetProcAddress(ntdll, "NtClose");
    }

    /// <summary>
    /// Retrieves the total contents of a directory.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    public static bool TryGetDirectoryContents(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        return TryGetDirectoryContents(path, files, directories);
    }

    /// <summary>
    /// Retrieves the total contents of a directory.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    public static bool TryGetDirectoryContents(string path, List<FileInformation> files, List<DirectoryInformation> directories)
    {
        path = Path.GetFullPath(path);
        return TryGetDirectoryContents_Internal(path, files.Add, directories.Add);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDirectoryContentsRecursive(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        GetDirectoryContentsRecursive(path, files, directories, false);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <param name="multithreaded">True to use Multithreading.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDirectoryContentsRecursive(string path, out List<FileInformation> files, out List<DirectoryInformation> directories, bool multithreaded)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        GetDirectoryContentsRecursive(path, files, directories, multithreaded);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDirectoryContentsRecursive(string path, List<FileInformation> files, List<DirectoryInformation> directories)
    {
        GetDirectoryContentsRecursive(path, files, directories, false);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <param name="multithreaded">True to use multithreading, else false.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDirectoryContentsRecursive(string path, List<FileInformation> files, List<DirectoryInformation> directories, bool multithreaded)
    {
        var initialDirectories = new List<DirectoryInformation>();

        path = Path.GetFullPath(path);
        var initialDirSuccess = TryGetDirectoryContents_Internal(path, files.Add, initialDirectories.Add);
        if (!initialDirSuccess)
            return;

        // Add initial directories
        directories.AddRange(initialDirectories);
        if (initialDirectories.Count <= 0)
            return;

        if (multithreaded)
        {
            // If multiple directories left, let's then go mutlithread.
            using var searcher = new MultithreadedDirectorySearcher(initialDirectories, files, directories, null);
            searcher.Run();
        }
        else
        {
            // Loop in single stack until all done.
            var remainingDirectories = new Stack<DirectoryInformation>(initialDirectories);
            while (remainingDirectories.TryPop(out var dir))
            {
                initialDirectories.Clear();
                TryGetDirectoryContents_Internal(dir.FullPath, files.Add, initialDirectories.Add);

                // Add to accumulator
                directories.AddRange(initialDirectories);

                // Add to remaining dirs
                foreach (var newDir in initialDirectories)
                    remainingDirectories.Push(newDir);
            }
        }
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="groups">Groupings of files to their corresponding directories.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetDirectoryContentsRecursiveGrouped(string path, out List<DirectoryFilesGroup> groups)
    {
        GetDirectoryContentsRecursiveGrouped(path, out groups, false);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="groups">Groupings of files to their corresponding directories.</param>
    /// <param name="multithreaded">True to use multithreading.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetDirectoryContentsRecursiveGrouped(string path, out List<DirectoryFilesGroup> groups, bool multithreaded)
    {
        groups = new List<DirectoryFilesGroup>();
        GetDirectoryContentsRecursiveGrouped(path, groups, multithreaded);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="groups">Groupings of files to their corresponding directories.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDirectoryContentsRecursiveGrouped(string path, List<DirectoryFilesGroup> groups) => GetDirectoryContentsRecursiveGrouped(path, groups, false);

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="groups">Groupings of files to their corresponding directories.</param>
    /// <param name="multithreaded">True to use multithreading, else false.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDirectoryContentsRecursiveGrouped(string path, List<DirectoryFilesGroup> groups, bool multithreaded)
    {
        var newFiles = new List<FileInformation>();
        var initialDirectories = new List<DirectoryInformation>();

        path = Path.GetFullPath(path);
        var initialDirSuccess = TryGetDirectoryContents_Internal(path, newFiles.Add, initialDirectories.Add);
        if (!initialDirSuccess)
            return;

        // Add initial files
        groups.Add(new DirectoryFilesGroup(new DirectoryInformation(Path.GetFullPath(path), Directory.GetLastWriteTime(path)), newFiles));
        if (initialDirectories.Count <= 0)
            return;

        // Loop in single stack until all done.
        if (multithreaded)
        {
            // If multiple directories left, let's then go mutlithread.
            using var searcher = new MultithreadedDirectorySearcher(initialDirectories, null, null, groups);
            searcher.Run();
        }
        else
        {
            var remainingDirectories = new Stack<DirectoryInformation>(initialDirectories);
            while (remainingDirectories.TryPop(out var dir))
            {
                newFiles.Clear();
                initialDirectories.Clear();
                TryGetDirectoryContents_Internal(dir.FullPath, newFiles.Add, initialDirectories.Add);

                // Add to accumulator
                groups.Add(new DirectoryFilesGroup(dir, newFiles));

                // Add to remaining dirs
                foreach (var newDir in initialDirectories)
                    remainingDirectories.Push(newDir);
            }
        }
    }

    #region Algorithm Constants
    // Stack allocated, so we need to restrict ourselves. I hope no file has longer name than 8000 character.
    const int StackBufferSize = 1024 * 16; 
    const uint STATUS_SUCCESS = 0x00000000;

    const uint FILE_ATTRIBUTE_NORMAL = 128;

    const int FILE_DIRECTORY_INFORMATION_CLASS = 1;

    const uint FILE_OPEN = 1;
    const int FILE_SYNCHRONOUS_IO_NONALERT = 0x00000020;

    const int FILE_LIST_DIRECTORY = 0x00000001;
    const int SYNCHRONIZE = 0x00100000;
    #endregion

    /// <summary>
    /// Retrieves the total contents of a directory for a single directory.
    /// </summary>
    /// <param name="dirPath">The path for which to get the directory for. Must be full path.</param>
    /// <param name="onAddFile">The files present in this directory.</param>
    /// <param name="onAddDirectory">The directories present in this directory.</param>
    /// <returns>True on success, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool TryGetDirectoryContents_Internal(string dirPath, Action<FileInformation> onAddFile, Action<DirectoryInformation> onAddDirectory)
    {
        // Note: Thanks to SkipLocalsInit, this memory is not zero'd so the allocation is virtually free.
        byte* bufferPtr = stackalloc byte[StackBufferSize];

        // Add prefix if needed.
        var originalDirPath = dirPath;
        if (!dirPath.StartsWith(Prefix))
            dirPath = $"{Prefix}{dirPath}";

        // Open the folder for reading.
        var hFolder = IntPtr.Zero;
        var objectAttributes = new OBJECT_ATTRIBUTES();
        var statusBlock = new IO_STATUS_BLOCK();
        long allocSize = 0;
        IntPtr result;

        fixed (char* dirString = dirPath)
        {
            var objectName = new UNICODE_STRING(dirString, dirPath.Length);
            objectAttributes.ObjectName = &objectName;
            result = NtCreateFile(ref hFolder, FILE_LIST_DIRECTORY | SYNCHRONIZE, ref objectAttributes, ref statusBlock, ref allocSize, FILE_ATTRIBUTE_NORMAL, FileShare.Read, FILE_DIRECTORY_INFORMATION_CLASS, FILE_OPEN | FILE_SYNCHRONOUS_IO_NONALERT, IntPtr.Zero, 0);
        }

        if ((ulong)result != STATUS_SUCCESS)
            return false;

        try
        {
            // Read remaining files while possible.
            bool moreFiles = true;
            while (moreFiles)
            {
                statusBlock = new IO_STATUS_BLOCK();
                var ntstatus = NtQueryDirectoryFile(hFolder,   // Our directory handle.
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref statusBlock,  // Pointers we don't care about 
                    (IntPtr)bufferPtr, StackBufferSize, FILE_DIRECTORY_INFORMATION_CLASS, // Buffer info.
                    0, IntPtr.Zero, 0);

                var currentBufferPtr = (IntPtr)bufferPtr;
                if (ntstatus != STATUS_SUCCESS)
                {
                    moreFiles = false;
                }
                else
                {
                    FILE_DIRECTORY_INFORMATION* info;
                    do
                    {
                        info = (FILE_DIRECTORY_INFORMATION*)currentBufferPtr;

                        // Not symlink or symlink to offline file.
                        if ((info->FileAttributes & FileAttributes.ReparsePoint) != 0 &&
                            (info->FileAttributes & FileAttributes.Offline) == 0)
                            goto nextfile;

                        var fileName = Marshal.PtrToStringUni(currentBufferPtr + sizeof(FILE_DIRECTORY_INFORMATION), (int)info->FileNameLength / 2);

                        if (fileName == "." || fileName == "..")
                            goto nextfile;

                        var isDirectory = (info->FileAttributes & FileAttributes.Directory) > 0;
                        if (isDirectory)
                        {
                            onAddDirectory(new DirectoryInformation
                            {
                                FullPath = $@"{originalDirPath}\{fileName}",
                                LastWriteTime = info->LastWriteTime.ToDateTime()
                            });
                        }
                        else if (!isDirectory)
                        {
                            onAddFile(new FileInformation
                            {
                                DirectoryPath = originalDirPath,
                                FileName = fileName,
                                LastWriteTime = info->LastWriteTime.ToDateTime()
                            });
                        }

                    nextfile:
                        currentBufferPtr += (int)info->NextEntryOffset;
                    }
                    while (info->NextEntryOffset != 0);
                }
            }
        }
        finally
        {
            NtClose(hFolder);
        }

        return true;
    }

    private readonly struct MultithreadedDirectorySearcher : IDisposable
    {
        private readonly bool[] _isIdle;
        private readonly ManualResetEventSlim _workDone;
        private readonly ManualResetEventSlim _threadStart;
        private readonly ConcurrentQueue<DirectoryInformation> _remainingDirectories;
        private readonly List<FileInformation>? _files;
        private readonly List<DirectoryInformation>? _directories;
        private readonly List<DirectoryFilesGroup>? _groups;
        private readonly SemaphoreSlim _singleThreadSemaphore = new(1);

        public MultithreadedDirectorySearcher(IEnumerable<DirectoryInformation> initialDirectories,
            List<FileInformation>? files, List<DirectoryInformation>? directories,
            List<DirectoryFilesGroup>? groups, int numThreads = -1)
        {
            if (numThreads == -1)
                numThreads = Math.Min(8, Environment.ProcessorCount * 2);

            _files = files;
            _directories = directories;
            _groups = groups;
            _workDone = new ManualResetEventSlim(false);
            _threadStart = new ManualResetEventSlim(false);
            _remainingDirectories = new ConcurrentQueue<DirectoryInformation>(initialDirectories);
            _isIdle = new bool[numThreads];
            var threads = GC.AllocateUninitializedArray<Thread>(numThreads);

            for (int x = 0; x < numThreads; x++)
            {
                var thread = new Thread(ThreadLogic, ushort.MaxValue * 2); // Limit stack to just what we need.
                thread.IsBackground = true;
                thread.Start(x);
                threads[x] = thread;
            }
        }

        public void Run()
        {
            _threadStart.Set();
            _workDone.Wait();
        }

        private bool AllThreadsIdle()
        {
            foreach (var completed in _isIdle)
            {
                if (!completed)
                    return false;
            }

            return true;
        }

        private void ThreadLogic(object? threadIdObj)
        {
            var threadId = (int)threadIdObj!;
            _threadStart.Wait();

            // Init
            var files = new List<FileInformation>();
            var directories = new List<DirectoryInformation>();
            var remainingDirectories = _remainingDirectories;
            
            List<DirectoryFilesGroup> groups = null!;
            if (_groups != null)
                groups = new List<DirectoryFilesGroup>();

            // Get cracking.
            ProcessItems:
            _isIdle[threadId] = false;
                
            // Process remaining directories
            while (remainingDirectories.TryDequeue(out var result))
            {
                TryGetDirectoryContents_Internal(result.FullPath, files.Add, information =>
                {
                    directories.Add(information);
                    remainingDirectories.Enqueue(information);
                });

                if (_groups == null) 
                    continue;
                
                // Make group and clear local data.
                groups.Add(new DirectoryFilesGroup(result, files.ToList()));
                files.Clear();
                directories.Clear();
            }
            
            // Only single thread can add to global list at same time.
            _singleThreadSemaphore.Wait();
            if (_groups == null)
            {
                _files!.AddRange(files);
                _directories!.AddRange(directories);
                files.Clear(); // clear state for possible resume after goto
                directories.Clear();
            }
            else
            {
                _groups.AddRange(groups);
                groups.Clear(); // clear state for possible resume after goto
            }
            
            _singleThreadSemaphore.Release();
            _isIdle[threadId] = true;
            
            // Wait for possible next item to process.
            while (!AllThreadsIdle())
            {
                if (remainingDirectories.Count > 0)
                    goto ProcessItems;
                
                Thread.Yield();
            }
            
            // All threads are done, we good.
            _workDone.Set();
        }

        public void Dispose()
        {
            _threadStart.Dispose();
            _workDone.Dispose();
            _singleThreadSemaphore.Dispose();
        }
    }

    #region Native Import Wrappers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr NtCreateFile(ref IntPtr handle, int access, ref OBJECT_ATTRIBUTES objectAttributes,
        ref IO_STATUS_BLOCK ioStatus, ref long allocSize, uint fileAttributes, FileShare share, int createDisposition,
        uint createOptions, IntPtr eaBuffer, uint eaLength)
    {
        return NtCreateFilePtr(ref handle, access, ref objectAttributes, ref ioStatus, ref allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint NtQueryDirectoryFile(IntPtr FileHandle, IntPtr Event, IntPtr ApcRoutine, IntPtr ApcContext,
        ref IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, uint Length, uint FileInformationClass, int BoolReturnSingleEntry,
        IntPtr FileName, int BoolRestartScan)
    {
        return NtQueryDirectoryFilePtr(FileHandle, Event, ApcRoutine, ApcContext, ref IoStatusBlock, FileInformation, Length, FileInformationClass, BoolReturnSingleEntry, FileName, BoolRestartScan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int NtClose(IntPtr hObject) => NtClosePtr(hObject);
    #endregion

    #region Native Structs
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    internal struct FILE_DIRECTORY_INFORMATION
    {
        internal uint NextEntryOffset;
        internal uint FileIndex;
        internal LARGE_INTEGER CreationTime;
        internal LARGE_INTEGER LastAccessTime;
        internal LARGE_INTEGER LastWriteTime;
        internal LARGE_INTEGER ChangeTime;
        internal LARGE_INTEGER EndOfFile;
        internal LARGE_INTEGER AllocationSize;
        internal FileAttributes FileAttributes;
        internal uint FileNameLength;
        // char[] fileName
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct IO_STATUS_BLOCK_UNION
    {
        [FieldOffset(0)]
        internal uint Status;
        [FieldOffset(0)]
        internal IntPtr Pointer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IO_STATUS_BLOCK
    {
        internal IO_STATUS_BLOCK_UNION Union;
        internal UIntPtr Information;
    }
    #endregion
}

/// <summary>
/// Represents information tied to an individual file.
/// </summary>
public struct FileInformation
{
    /// <summary>
    /// Path to the directory containing this file.
    /// </summary>
    public string DirectoryPath;

    /// <summary>
    /// Name of the file relative to directory.
    /// </summary>
    public string FileName;

    /// <summary>
    /// Last time this file was written to.
    /// </summary>
    public DateTime LastWriteTime;

    /// <inheritdoc/>
    public override string ToString() => FileName;
}

/// <summary>
/// Represents information tied to an individual directory.
/// </summary>
public struct DirectoryInformation
{
    /// <summary>
    /// Full path to the directory.
    /// </summary>
    public string FullPath;

    /// <summary>
    /// Last time this directory was modified.
    /// </summary>
    public DateTime LastWriteTime;

    public DirectoryInformation(string fullPath, DateTime lastWriteTime)
    {
        FullPath = fullPath;
        LastWriteTime = lastWriteTime;
    }

    /// <inheritdoc/>
    public override string ToString() => FullPath;
}

/// <summary>
/// Groups a single directory and a list of files associated with it.
/// </summary>
public class DirectoryFilesGroup
{
    /// <summary>
    /// The directory in question.
    /// </summary>
    public DirectoryInformation Directory;

    /// <summary>
    /// The relative file paths tied to this directory.
    /// </summary>
    public string[] Files;

    /// <summary/>
    public DirectoryFilesGroup(DirectoryInformation directory, List<FileInformation> files)
    {
        Directory = directory;
        Files = GC.AllocateUninitializedArray<string>(files.Count);
        for (int x = 0; x < files.Count; x++)
            Files[x] = files[x].FileName;
    }

    /// <inheritdoc/>
    public override string ToString() => Directory.FullPath;
}