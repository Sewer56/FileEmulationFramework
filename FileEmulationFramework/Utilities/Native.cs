﻿using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Memory.Pointers;
using static FileEmulationFramework.Lib.Utilities.Native;

// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming

namespace FileEmulationFramework.Utilities;

public class Native
{
    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct CloseHandleFn
    {
        public FuncPtr<
            IntPtr, // handle 
            NT_STATUS // status
        > Value;
    }
    
    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct NtCreateFileFn
    {
        public FuncPtr<
            Ptr<IntPtr>, // handle 
            FileAccess, // access
            Ptr<OBJECT_ATTRIBUTES>, // objectAttributes
            Ptr<IO_STATUS_BLOCK>, // ioStatus
            Ptr<long>, // allocSize
            uint, // fileAttributes
            FileShare, // share
            uint, // createDisposition
            uint, // createOptions
            IntPtr, // eaBuffer
            uint, // eaLength
            NT_STATUS // status
        > Value;
    }

    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct NtReadFileFn
    {
        public FuncPtr<
            IntPtr, // handle 
            IntPtr, // hEvent
            Ptr<IntPtr>, // apcRoutine
            Ptr<IntPtr>, // apcContext
            Ptr<IO_STATUS_BLOCK>, // ioStatus
            Ptr<byte>, // buffer
            uint, // length
            Ptr<long>, // byteOffset
            IntPtr, // key
            NT_STATUS // status
        > Value;
    }
    
    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct NtSetInformationFileFn
    {
        public FuncPtr<
            IntPtr, // hFile 
            Ptr<IO_STATUS_BLOCK>, // ioStatus
            Ptr<byte>,   // fileInformation
            uint, // length
            FileInformationClass, // fileInformationClass
            NT_STATUS // status
        > Value;
    }
    
    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct NtQueryInformationFileFn
    {
        public FuncPtr<
            IntPtr, // hFile 
            Ptr<IO_STATUS_BLOCK>, // ioStatus
            Ptr<byte>,   // fileInformation
            uint, // length
            FileInformationClass, // fileInformationClass
            NT_STATUS // status
        > Value;
    }

    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct NtQueryAttributesFileFn
    {
        public FuncPtr<
            Ptr<OBJECT_ATTRIBUTES>, // attributes
            Ptr<FILE_BASIC_INFORMATION>, // information
            int // status
        > Value;
    }

    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct NtQueryFullAttributesFileFn
    {
        public FuncPtr<
            Ptr<OBJECT_ATTRIBUTES>, // attributes
            Ptr<FILE_NETWORK_OPEN_INFORMATION>, // information
            int // status
        > Value;
    }


    /// <summary>
    /// A driver sets an IRP's I/O status block to indicate the final status of an I/O request, before calling IoCompleteRequest for the IRP.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IO_STATUS_BLOCK
    {
        public NT_STATUS Status;
        public IntPtr Information;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILE_STANDARD_INFORMATION
    {
        public long AllocationSize;
        public long EndOfFile;
        public uint NumberOfLinks;
        public bool DeletePending;
        public bool Directory;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILE_BASIC_INFORMATION
    {
        public LARGE_INTEGER CreationTime;
        public LARGE_INTEGER LastAccessTime;
        public LARGE_INTEGER LastWriteTime;
        public LARGE_INTEGER ChangeTime;
        public ulong FileAttributes;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct FILE_NETWORK_OPEN_INFORMATION
    {
        public LARGE_INTEGER CreationTime;
        public LARGE_INTEGER LastAccessTime;
        public LARGE_INTEGER LastWriteTime;
        public LARGE_INTEGER ChangeTime;
        public LARGE_INTEGER AllocationSize;
        public long EndOfFile;
        public ulong FileAttributes;
    }


    /// <summary>
    ///     Enumeration of the various file information classes.
    ///     See wdm.h.
    /// </summary>
    public enum FileInformationClass
    {
#pragma warning disable CS1591
        None = 0,
        FileDirectoryInformation = 1,
        FileFullDirectoryInformation, // 2
        FileBothDirectoryInformation, // 3
        FileBasicInformation, // 4
        FileStandardInformation, // 5
        FileInternalInformation, // 6
        FileEaInformation, // 7
        FileAccessInformation, // 8
        FileNameInformation, // 9
        FileRenameInformation, // 10
        FileLinkInformation, // 11
        FileNamesInformation, // 12
        FileDispositionInformation, // 13
        FilePositionInformation, // 14
        FileFullEaInformation, // 15
        FileModeInformation, // 16
        FileAlignmentInformation, // 17
        FileAllInformation, // 18
        FileAllocationInformation, // 19
        FileEndOfFileInformation, // 20
        FileAlternateNameInformation, // 21
        FileStreamInformation, // 22
        FilePipeInformation, // 23
        FilePipeLocalInformation, // 24
        FilePipeRemoteInformation, // 25
        FileMailslotQueryInformation, // 26
        FileMailslotSetInformation, // 27
        FileCompressionInformation, // 28
        FileObjectIdInformation, // 29
        FileCompletionInformation, // 30
        FileMoveClusterInformation, // 31
        FileQuotaInformation, // 32
        FileReparsePointInformation, // 33
        FileNetworkOpenInformation, // 34
        FileAttributeTagInformation, // 35
        FileTrackingInformation, // 36
        FileIdBothDirectoryInformation, // 37
        FileIdFullDirectoryInformation, // 38
        FileValidDataLengthInformation, // 39
        FileShortNameInformation, // 40
        FileIoCompletionNotificationInformation, // 41
        FileIoStatusBlockRangeInformation, // 42
        FileIoPriorityHintInformation, // 43
        FileSfioReserveInformation, // 44
        FileSfioVolumeInformation, // 45
        FileHardLinkInformation, // 46
        FileProcessIdsUsingFileInformation, // 47
        FileNormalizedNameInformation, // 48
        FileNetworkPhysicalNameInformation, // 49
        FileIdGlobalTxDirectoryInformation, // 50
        FileIsRemoteDeviceInformation, // 51
        FileAttributeCacheInformation, // 52
        FileNumaNodeInformation, // 53
        FileStandardLinkInformation, // 54
        FileRemoteProtocolInformation, // 55
        FileMaximumInformation,
#pragma warning restore CS1591
    }

    /// <summary>
    /// An enumeration of Status values returned by functions.
    /// There are a lot so only including ones that we actually need to use.
    /// </summary>
    public enum NT_STATUS : uint
    {
        #pragma warning disable CS1591
        STATUS_SUCCESS = 0,
        STATUS_END_OF_FILE = 0xC0000011,
#pragma warning restore CS1591
    }
    
}