namespace FileEmulationFramework.Lib.Utilities;

internal unsafe struct MemoryMapFunctions
{
    public static MemoryMapFunctions Instance = new MemoryMapFunctions();

    private delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint, IntPtr, IntPtr, IntPtr> _mapViewOfFileEx;
    private delegate* unmanaged[Stdcall]<IntPtr, uint, uint> _unMapViewOfFileEx;

    /// <summary>
    /// Do not call this constructor. Use <see cref="Instance"/>.
    /// </summary>
    public MemoryMapFunctions()
    {
        var krn32Handle = Native.LoadLibrary("kernel32.dll");
        _mapViewOfFileEx = (delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint, IntPtr, IntPtr, IntPtr>)Native.GetProcAddress(krn32Handle, "MapViewOfFileEx");
        _unMapViewOfFileEx = (delegate* unmanaged[Stdcall]<IntPtr, uint, uint>)Native.GetProcAddress(krn32Handle, "UnmapViewOfFileEx");
    }

    /// <summary>
    /// <para>
    /// Maps a view of a file mapping into the address space of a calling process. A caller can optionally specify a suggested base
    /// memory address for the view.
    /// </para>
    /// <para>To specify the NUMA node for the physical memory, see <c>MapViewOfFileExNuma</c>.</para>
    /// </summary>
    /// <param name="hFileMappingObject">
    /// A handle to a file mapping object. The <c>CreateFileMapping</c> and <c>OpenFileMapping</c> functions return this handle.
    /// </param>
    /// <param name="dwDesiredAccess">
    /// <para>
    /// The type of access to a file mapping object, which determines the page protection of the pages. This parameter can be one of the
    /// following values.
    /// </para>
    /// <para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <term>Meaning</term>
    /// </listheader>
    /// <item>
    /// <term>FILE_MAP_ALL_ACCESS</term>
    /// <term>
    /// A read/write view of the file is mapped. The file mapping object must have been created with PAGE_READWRITE or
    /// PAGE_EXECUTE_READWRITE protection.When used with the MapViewOfFileEx function, FILE_MAP_ALL_ACCESS is equivalent to FILE_MAP_WRITE.
    /// </term>
    /// </item>
    /// <item>
    /// <term>FILE_MAP_COPY</term>
    /// <term>
    /// A copy-on-write view of the file is mapped. The file mapping object must have been created with PAGE_READONLY, PAGE_READ_EXECUTE,
    /// PAGE_WRITECOPY, PAGE_EXECUTE_WRITECOPY, PAGE_READWRITE, or PAGE_EXECUTE_READWRITE protection.When a process writes to a
    /// copy-on-write page, the system copies the original page to a new page that is private to the process. The new page is backed by
    /// the paging file. The protection of the new page changes from copy-on-write to read/write.When copy-on-write access is specified,
    /// the system and process commit charge taken is for the entire view because the calling process can potentially write to every page
    /// in the view, making all pages private. The contents of the new page are never written back to the original file and are lost when
    /// the view is unmapped.
    /// </term>
    /// </item>
    /// <item>
    /// <term>FILE_MAP_READ</term>
    /// <term>
    /// A read-only view of the file is mapped. An attempt to write to the file view results in an access violation.The file mapping
    /// object must have been created with PAGE_READONLY, PAGE_READWRITE, PAGE_EXECUTE_READ, or PAGE_EXECUTE_READWRITE protection.
    /// </term>
    /// </item>
    /// <item>
    /// <term>FILE_MAP_WRITE</term>
    /// <term>
    /// A read/write view of the file is mapped. The file mapping object must have been created with PAGE_READWRITE or
    /// PAGE_EXECUTE_READWRITE protection.When used with MapViewOfFileEx, (FILE_MAP_WRITE | FILE_MAP_READ) and FILE_MAP_ALL_ACCESS are
    /// equivalent to FILE_MAP_WRITE.
    /// </term>
    /// </item>
    /// </list>
    /// </para>
    /// <para>Each of the preceding values can be combined with the following value.</para>
    /// <para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <term>Meaning</term>
    /// </listheader>
    /// <item>
    /// <term>FILE_MAP_EXECUTE</term>
    /// <term>
    /// An executable view of the file is mapped (mapped memory can be run as code). The file mapping object must have been created with
    /// PAGE_EXECUTE_READ, PAGE_EXECUTE_WRITECOPY, or PAGE_EXECUTE_READWRITE protection.Windows Server 2003 and Windows XP: This value is
    /// available starting with Windows XP with SP2 and Windows Server 2003 with SP1.
    /// </term>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// For file mapping objects created with the <c>SEC_IMAGE</c> attribute, the dwDesiredAccess parameter has no effect and should be
    /// set to any valid value such as <c>FILE_MAP_READ</c>.
    /// </para>
    /// <para>For more information about access to file mapping objects, see File Mapping Security and Access Rights.</para>
    /// </param>
    /// <param name="dwFileOffsetHigh">The high-order <c>DWORD</c> of the file offset where the view is to begin.</param>
    /// <param name="dwFileOffsetLow">
    /// The low-order <c>DWORD</c> of the file offset where the view is to begin. The combination of the high and low offsets must
    /// specify an offset within the file mapping. They must also match the memory allocation granularity of the system. That is, the
    /// offset must be a multiple of the allocation granularity. To obtain the memory allocation granularity of the system, use the
    /// <c>GetSystemInfo</c> function, which fills in the members of a <c>SYSTEM_INFO</c> structure.
    /// </param>
    /// <param name="dwNumberOfBytesToMap">
    /// The number of bytes of a file mapping to map to a view. All bytes must be within the maximum size specified by
    /// <c>CreateFileMapping</c>. If this parameter is 0 (zero), the mapping extends from the specified offset to the end of the file mapping.
    /// </param>
    /// <param name="lpBaseAddress">
    /// <para>
    /// A pointer to the memory address in the calling process address space where mapping begins. This must be a multiple of the
    /// system's memory allocation granularity, or the function fails. To determine the memory allocation granularity of the system, use
    /// the <c>GetSystemInfo</c> function. If there is not enough address space at the specified address, the function fails.
    /// </para>
    /// <para>
    /// If lpBaseAddress is <c>NULL</c>, the operating system chooses the mapping address. In this scenario, the function is equivalent
    /// to the <c>MapViewOfFile</c> function.
    /// </para>
    /// <para>
    /// While it is possible to specify an address that is safe now (not used by the operating system), there is no guarantee that the
    /// address will remain safe over time. Therefore, it is better to let the operating system choose the address. In this case, you
    /// would not store pointers in the memory mapped file, you would store offsets from the base of the file mapping so that the mapping
    /// can be used at any address.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>If the function succeeds, the return value is the starting address of the mapped view.</para>
    /// <para>If the function fails, the return value is <c>NULL</c>. To get extended error information, call <c>GetLastError</c>.</para>
    /// </returns>
    public IntPtr MapViewOfFileEx(IntPtr hFileMappingObject, Native.FILE_MAP dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap, IntPtr lpBaseAddress)
    {
        return _mapViewOfFileEx(hFileMappingObject, (uint)dwDesiredAccess, dwFileOffsetHigh, dwFileOffsetLow, (IntPtr)dwNumberOfBytesToMap, lpBaseAddress);
    }

    /// <summary>This is an extended version of UnmapViewOfFile that takes an additional flags parameter.</summary>
    /// <param name="baseAddress">
    /// A pointer to the base address of the mapped view of a file that is to be unmapped. This value must be identical to the value
    /// returned by a previous call to the MapViewOfFile or MapViewOfFileEx function.
    /// </param>
    /// <param name="unmapFlags">
    /// The only supported flag is MEM_UNMAP_WITH_TRANSIENT_BOOST (0x1), which specifies that the priority of the pages being unmapped
    /// should be temporarily boosted because the caller expects that these pages will be accessed again shortly. For more information
    /// about memory priorities, see the SetThreadInformation(ThreadMemoryPriority) function.
    /// </param>
    /// <returns>
    /// <para>If the function succeeds, the return value is a nonzero value.</para>
    /// <para>If the function fails, the return value is 0 (zero). To get extended error information, call <c>GetLastError</c>.</para>
    /// </returns>
    public bool UnmapViewOfFileEx(IntPtr baseAddress, uint unmapFlags)
    {
        return _unMapViewOfFileEx(baseAddress, unmapFlags) != 0;
    }
}
