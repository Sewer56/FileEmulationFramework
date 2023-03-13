// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static FileEmulationFramework.Lib.Utilities.Native;

#pragma warning disable CS1591

namespace FileEmulationFramework.Benchmarks.Legacy;

// Legacy implementation for benchmarking comparison.
// Legacy being from Reloaded.IO.

/// <summary>
/// Class that provides WinAPI based utility methods for fast file enumeration in directories.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Taken from Reloaded.IO. Known good implementation.")]
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
        return TryGetDirectoryContents_Internal(path, files, directories);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetDirectoryContentsRecursive(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        GetDirectoryContentsRecursive(path, files, directories);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetDirectoryContentsRecursive(string path, List<FileInformation> files, List<DirectoryInformation> directories)
    {
        var newFiles = new List<FileInformation>();
        var initialDirectories = new List<DirectoryInformation>();

        path = Path.GetFullPath(path);
        var initialDirSuccess = TryGetDirectoryContents_Internal(path, newFiles, initialDirectories);
        if (!initialDirSuccess)
            return;

        // Add initial files
        files.AddRange(newFiles);
        directories.AddRange(initialDirectories);
        if (initialDirectories.Count <= 0)
            return;

        // Loop in single stack until all done.
        var remainingDirectories = new Stack<DirectoryInformation>(initialDirectories);
        while (remainingDirectories.TryPop(out var dir))
        {
            newFiles.Clear();
            initialDirectories.Clear();
            TryGetDirectoryContents_Internal(dir.FullPath, newFiles, initialDirectories);

            // Add to accumulator
            directories.AddRange(initialDirectories);
            files.AddRange(newFiles);

            // Add to remaining dirs
            foreach (var newDir in initialDirectories)
                remainingDirectories.Push(newDir);
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
        groups = new List<DirectoryFilesGroup>();
        GetDirectoryContentsRecursiveGrouped(path, groups);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="groups">Groupings of files to their corresponding directories.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetDirectoryContentsRecursiveGrouped(string path, List<DirectoryFilesGroup> groups)
    {
        var newFiles = new List<FileInformation>();
        var initialDirectories = new List<DirectoryInformation>();

        path = Path.GetFullPath(path);
        var initialDirSuccess = TryGetDirectoryContents_Internal(path, newFiles, initialDirectories);
        if (!initialDirSuccess)
            return;

        // Add initial files
        groups.Add(new DirectoryFilesGroup(new DirectoryInformation(Path.GetFullPath(path), Directory.GetLastWriteTime(path)), newFiles));
        if (initialDirectories.Count <= 0)
            return;

        // Loop in single stack until all done.
        var remainingDirectories = new Stack<DirectoryInformation>(initialDirectories);
        while (remainingDirectories.TryPop(out var dir))
        {
            newFiles.Clear();
            initialDirectories.Clear();
            TryGetDirectoryContents_Internal(dir.FullPath, newFiles, initialDirectories);

            // Add to accumulator
            groups.Add(new DirectoryFilesGroup(dir, newFiles));

            // Add to remaining dirs
            foreach (var newDir in initialDirectories)
                remainingDirectories.Push(newDir);
        }
    }

    /// <summary>
    /// Retrieves the total contents of a directory for a single directory.
    /// </summary>
    /// <param name="dirPath">The path for which to get the directory for. Must be full path.</param>
    /// <param name="files">The files present in this directory.</param>
    /// <param name="directories">The directories present in this directory.</param>
    /// <returns>True on success, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe bool TryGetDirectoryContents_Internal(string dirPath, List<FileInformation> files, List<DirectoryInformation> directories)
    {
        // 128K seemed to have good enough size to fill most queries in while still preserving stack.
        const int BufferSize = 1024 * 16;
        const uint STATUS_SUCCESS = 0x00000000;

        const uint FILE_ATTRIBUTE_NORMAL = 128;

        const int FILE_DIRECTORY_INFORMATION = 1;

        const uint FILE_OPEN = 1;
        const int FILE_SYNCHRONOUS_IO_NONALERT = 0x00000020;

        const int FILE_LIST_DIRECTORY = 0x00000001;
        const int SYNCHRONIZE = 0x00100000;

        // Note: Thanks to SkipLocalsInit, this memory is not zero'd so the allocation is virtually free.
        byte* bufferPtr = stackalloc byte[BufferSize];

        // Add prefix if needed.
        var originalDirPath = dirPath;
        if (!dirPath.StartsWith(Prefix))
            dirPath = $"{Prefix}{dirPath}";

        // Open the folder for reading.
        var hFolder = IntPtr.Zero;
        var objectAttributes = new OBJECT_ATTRIBUTES
        {
            Length = sizeof(OBJECT_ATTRIBUTES),
            Attributes = 0,
            RootDirectory = IntPtr.Zero,
            SecurityDescriptor = IntPtr.Zero,
            SecurityQualityOfService = IntPtr.Zero
        };

        var statusBlock = new IO_STATUS_BLOCK();
        long allocSize = 0;
        var result = IntPtr.Zero;

        fixed (char* dirString = dirPath)
        {
            var objectName = new UNICODE_STRING(dirString, dirPath.Length);
            objectAttributes.ObjectName = &objectName;

            result = NtCreateFile(ref hFolder, FILE_LIST_DIRECTORY | SYNCHRONIZE, ref objectAttributes, ref statusBlock, ref allocSize, FILE_ATTRIBUTE_NORMAL, FileShare.Read, FILE_DIRECTORY_INFORMATION, FILE_OPEN | FILE_SYNCHRONOUS_IO_NONALERT, IntPtr.Zero, 0);
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
                    (IntPtr)bufferPtr, BufferSize, FILE_DIRECTORY_INFORMATION, // Buffer info.
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
                            directories.Add(new DirectoryInformation
                            {
                                FullPath = $@"{originalDirPath}\{fileName}",
                                LastWriteTime = info->LastWriteTime.ToDateTime()
                            });
                        }
                        else if (!isDirectory)
                        {
                            files.Add(new FileInformation
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

    #region Native Import Wrappers
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe IntPtr NtCreateFile(ref IntPtr handle, int access, ref OBJECT_ATTRIBUTES objectAttributes,
        ref IO_STATUS_BLOCK ioStatus, ref long allocSize, uint fileAttributes, FileShare share, int createDisposition,
        uint createOptions, IntPtr eaBuffer, uint eaLength)
    {
        return NtCreateFilePtr(ref handle, access, ref objectAttributes, ref ioStatus, ref allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe uint NtQueryDirectoryFile(IntPtr FileHandle, IntPtr Event, IntPtr ApcRoutine, IntPtr ApcContext,
        ref IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, uint Length, uint FileInformationClass, int BoolReturnSingleEntry,
        IntPtr FileName, int BoolRestartScan)
    {
        return NtQueryDirectoryFilePtr(FileHandle, Event, ApcRoutine, ApcContext, ref IoStatusBlock, FileInformation, Length, FileInformationClass, BoolReturnSingleEntry, FileName, BoolRestartScan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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