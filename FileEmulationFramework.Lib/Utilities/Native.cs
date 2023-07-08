using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable InconsistentNaming

namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Exposes some common useful native functions.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Definitions for Interop.")]
public static class Native
{
    /// <summary>
    /// <para>
    /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
    /// </para>
    /// <para>For additional load options, use the <c>LoadLibraryEx</c> function.</para>
    /// </summary>
    /// <param name="lpFileName">
    /// <para>
    /// The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name
    /// specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the
    /// <c>LIBRARY</c> keyword in the module-definition (.def) file.
    /// </para>
    /// <para>If the string specifies a full path, the function searches only that path for the module.</para>
    /// <para>
    /// If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the
    /// module; for more information, see the Remarks.
    /// </para>
    /// <para>
    /// If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\), not forward
    /// slashes (/). For more information about paths, see Naming a File or Directory.
    /// </para>
    /// <para>
    /// If the string specifies a module name without a path and the file name extension is omitted, the function appends the default
    /// library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing
    /// point character (.) in the module name string.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>If the function succeeds, the return value is a handle to the module.</para>
    /// <para>If the function fails, the return value is NULL. To get extended error information, call <c>GetLastError</c>.</para>
    /// </returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern IntPtr LoadLibrary([In, MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

    /// <summary>Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).</summary>
    /// <param name="hModule">
    /// <para>
    /// A handle to the DLL module that contains the function or variable. The <c>LoadLibrary</c>, <c>LoadLibraryEx</c>,
    /// <c>LoadPackagedLibrary</c>, or <c>GetModuleHandle</c> function returns this handle.
    /// </para>
    /// <para>
    /// The <c>GetProcAddress</c> function does not retrieve addresses from modules that were loaded using the
    /// <c>LOAD_LIBRARY_AS_DATAFILE</c> flag. For more information, see <c>LoadLibraryEx</c>.
    /// </para>
    /// </param>
    /// <param name="lpProcName">
    /// The function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the
    /// low-order word; the high-order word must be zero.
    /// </param>
    /// <returns>
    /// <para>If the function succeeds, the return value is the address of the exported function or variable.</para>
    /// <para>If the function fails, the return value is NULL. To get extended error information, call <c>GetLastError</c>.</para>
    /// </returns>
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

    /// <summary>
    /// <para>Moves the file pointer of the specified file.</para>
    /// </summary>
    /// <param name="hFile">
    /// <para>
    /// A handle to the file. The file handle must have been created with the <c>GENERIC_READ</c> or <c>GENERIC_WRITE</c> access right.
    /// For more information, see File Security and Access Rights.
    /// </para>
    /// </param>
    /// <param name="liDistanceToMove">
    /// <para>
    /// The number of bytes to move the file pointer. A positive value moves the pointer forward in the file and a negative value moves
    /// the file pointer backward.
    /// </para>
    /// </param>
    /// <param name="lpNewFilePointer">
    /// <para>
    /// A pointer to a variable to receive the new file pointer. If this parameter is <c>NULL</c>, the new file pointer is not returned.
    /// </para>
    /// </param>
    /// <param name="dwMoveMethod">
    /// <para>The starting point for the file pointer move. This parameter can be one of the following values.</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <term>Meaning</term>
    /// </listheader>
    /// <item>
    /// <term>FILE_BEGIN = 0</term>
    /// <term>
    /// The starting point is zero or the beginning of the file. If this flag is specified, then the liDistanceToMove parameter is
    /// interpreted as an unsigned value.
    /// </term>
    /// </item>
    /// <item>
    /// <term>FILE_CURRENT = 1</term>
    /// <term>The start point is the current value of the file pointer.</term>
    /// </item>
    /// <item>
    /// <term>FILE_END = 2</term>
    /// <term>The starting point is the current end-of-file position.</term>
    /// </item>
    /// </list>
    /// </param>
    /// <returns>
    /// <para>If the function succeeds, the return value is nonzero.</para>
    /// <para>If the function fails, the return value is zero. To get extended error information, call <c>GetLastError</c>.</para>
    /// </returns>
    [DllImport("kernel32.dll")]
    public static extern int SetFilePointerEx(IntPtr hFile, long liDistanceToMove, IntPtr lpNewFilePointer, uint dwMoveMethod);

    /// <summary>
    /// Reads data from the specified file or input/output (I/O) device. Reads occur at the position specified by the file pointer if
    /// supported by the device.
    /// </summary>
    /// <param name="hFile">
    /// A handle to the device (for example, a file, file stream, physical disk, volume, console buffer, tape drive, socket,
    /// communications resource, mailslot, or pipe). The hFile parameter must have been created with read access.
    /// </param>
    /// <param name="lpBuffer">A pointer to the buffer that receives the data read from a file or device.</param>
    /// <param name="nNumberOfBytesToRead">The maximum number of bytes to be read.</param>
    /// <param name="lpNumberOfBytesRead">
    /// A pointer to the variable that receives the number of bytes read when using a synchronous hFile parameter. ReadFile sets this
    /// value to zero before doing any work or error checking. Use NULL for this parameter if this is an asynchronous operation to avoid
    /// potentially erroneous results.
    /// <para>This parameter can be NULL only when the lpOverlapped parameter is not NULL.</para>
    /// </param>
    /// <param name="lpOverlapped">
    /// A pointer to an OVERLAPPED structure is required if the hFile parameter was opened with FILE_FLAG_OVERLAPPED, otherwise it can be NULL.
    /// <para>
    /// If hFile is opened with FILE_FLAG_OVERLAPPED, the lpOverlapped parameter must point to a valid and unique OVERLAPPED structure,
    /// otherwise the function can incorrectly report that the read operation is complete.
    /// </para>
    /// <para>
    /// For an hFile that supports byte offsets, if you use this parameter you must specify a byte offset at which to start reading from
    /// the file or device. This offset is specified by setting the Offset and OffsetHigh members of the OVERLAPPED structure. For an
    /// hFile that does not support byte offsets, Offset and OffsetHigh are ignored.
    /// </para>
    /// <para>
    /// For more information about different combinations of lpOverlapped and FILE_FLAG_OVERLAPPED, see the Remarks section and the
    /// Synchronization and File Position section.
    /// </para>
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero (TRUE). If the function fails, or is completing asynchronously, the return
    /// value is zero(FALSE). To get extended error information, call the GetLastError function.
    /// </returns>
    [DllImport("kernel32.dll")]
    public static extern unsafe bool ReadFile(IntPtr hFile, byte* lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    /// <summary>
    /// Creates or opens a file or I/O device. The most commonly used I/O devices are as follows: file, file stream, directory, physical
    /// disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe. The function returns a handle that can be
    /// used to access the file or device for various types of I/O depending on the file or device and the flags and attributes specified.
    /// </summary>
    /// <param name="filename">
    /// The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.
    /// <para>
    /// In the ANSI version of this function, the name is limited to MAX_PATH characters. To extend this limit to 32,767 wide characters,
    /// call the Unicode version of the function and prepend "\\?\" to the path. For more information, see Naming Files, Paths, and Namespaces.
    /// </para>
    /// <para>For information on special device names, see Defining an MS-DOS Device Name.</para>
    /// <para>
    /// To create a file stream, specify the name of the file, a colon, and then the name of the stream. For more information, see File Streams.
    /// </para>
    /// <note type="tip">Starting with Windows 10, version 1607, for the Unicode version of this function (CreateFileW), you can opt-in
    /// to remove the MAX_PATH limitation without prepending "\\?\". See the "Maximum Path Length Limitation" section of Naming Files,
    /// Paths, and Namespaces for details.</note>
    /// </param>
    /// <param name="access">
    /// The requested access to the file or device, which can be summarized as read, write, both or neither zero).
    /// <para>
    /// The most commonly used values are GENERIC_READ, GENERIC_WRITE, or both (GENERIC_READ | GENERIC_WRITE). For more information, see
    /// Generic Access Rights, File Security and Access Rights, File Access Rights Constants, and ACCESS_MASK.
    /// </para>
    /// <para>
    /// If this parameter is zero, the application can query certain metadata such as file, directory, or device attributes without
    /// accessing that file or device, even if GENERIC_READ access would have been denied.
    /// </para>
    /// <para>
    /// You cannot request an access mode that conflicts with the sharing mode that is specified by the dwShareMode parameter in an open
    /// request that already has an open handle.
    /// </para>
    /// </param>
    /// <param name="share">
    /// The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none (refer to the
    /// following table). Access requests to attributes or extended attributes are not affected by this flag.
    /// <para>
    /// If this parameter is zero and CreateFile succeeds, the file or device cannot be shared and cannot be opened again until the
    /// handle to the file or device is closed. For more information, see the Remarks section.
    /// </para>
    /// <para>
    /// You cannot request a sharing mode that conflicts with the access mode that is specified in an existing request that has an open
    /// handle. CreateFile would fail and the GetLastError function would return ERROR_SHARING_VIOLATION.
    /// </para>
    /// <para>
    /// To enable a process to share a file or device while another process has the file or device open, use a compatible combination of
    /// one or more of the following values. For more information about valid combinations of this parameter with the dwDesiredAccess
    /// parameter, see Creating and Opening Files.
    /// </para>
    /// <note>The sharing options for each open handle remain in effect until that handle is closed, regardless of process context.</note>
    /// </param>
    /// <param name="securityAttributes">
    /// A pointer to a SECURITY_ATTRIBUTES structure that contains two separate but related data members: an optional security
    /// descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes.
    /// <para>This parameter can be NULL.</para>
    /// <para>
    /// If this parameter is NULL, the handle returned by CreateFile cannot be inherited by any child processes the application may
    /// create and the file or device associated with the returned handle gets a default security descriptor.
    /// </para>
    /// <para>
    /// The lpSecurityDescriptor member of the structure specifies a SECURITY_DESCRIPTOR for a file or device. If this member is NULL,
    /// the file or device associated with the returned handle is assigned a default security descriptor.
    /// </para>
    /// <para>
    /// CreateFile ignores the lpSecurityDescriptor member when opening an existing file or device, but continues to use the
    /// bInheritHandle member.
    /// </para>
    /// <para>The bInheritHandlemember of the structure specifies whether the returned handle can be inherited.</para>
    /// </param>
    /// <param name="creationDisposition">
    /// An action to take on a file or device that exists or does not exist.
    /// <para>For devices other than files, this parameter is usually set to OPEN_EXISTING.</para>
    /// </param>
    /// <param name="flagsAndAttributes">
    /// The file or device attributes and flags, FILE_ATTRIBUTE_NORMAL being the most common default value for files.
    /// <para>
    /// This parameter can include any combination of the available file attributes (FILE_ATTRIBUTE_*). All other file attributes
    /// override FILE_ATTRIBUTE_NORMAL.
    /// </para>
    /// <para>
    /// This parameter can also contain combinations of flags (FILE_FLAG_*) for control of file or device caching behavior, access modes,
    /// and other special-purpose flags. These combine with any FILE_ATTRIBUTE_* values.
    /// </para>
    /// <para>
    /// This parameter can also contain Security Quality of Service (SQOS) information by specifying the SECURITY_SQOS_PRESENT flag.
    /// Additional SQOS-related flags information is presented in the table following the attributes and flags tables.
    /// </para>
    /// <note>When CreateFile opens an existing file, it generally combines the file flags with the file attributes of the existing file,
    /// and ignores any file attributes supplied as part of dwFlagsAndAttributes. Special cases are detailed in Creating and Opening Files.</note>
    /// <para>
    /// Some of the following file attributes and flags may only apply to files and not necessarily all other types of devices that
    /// CreateFile can open. For additional information, see the Remarks section of this topic and Creating and Opening Files.
    /// </para>
    /// <para>
    /// For more advanced access to file attributes, see SetFileAttributes. For a complete list of all file attributes with their values
    /// and descriptions, see File Attribute Constants.
    /// </para>
    /// </param>
    /// <param name="templateFile">
    /// A valid handle to a template file with the GENERIC_READ access right. The template file supplies file attributes and extended
    /// attributes for the file that is being created.
    /// <para>This parameter can be NULL.</para>
    /// <para>When opening an existing file, CreateFile ignores this parameter.</para>
    /// <para>
    /// When opening a new encrypted file, the file inherits the discretionary access control list from its parent directory. For
    /// additional information, see File Encryption.
    /// </para>
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.
    /// <para>If the function fails, the return value is INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.</para>
    /// </returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateFileW(string filename, FileAccess access, FileShare share, IntPtr securityAttributes, FileMode creationDisposition, FileAttributes flagsAndAttributes, IntPtr templateFile);

    /// <summary>
    /// Closes an open object handle.
    /// </summary>
    /// <param name="hObject">A valid handle to an open object.</param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport("kernel32.dll")]
    public static extern IntPtr CloseHandle(IntPtr hObject);
    
    /// <summary>
    /// The OBJECT_ATTRIBUTES structure specifies attributes that can be applied to objects or object
    /// handles by routines that create objects and/or return handles to objects.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct OBJECT_ATTRIBUTES
    {
        /// <summary>
        /// Lengthm of this structure.
        /// </summary>
        public int Length = sizeof(OBJECT_ATTRIBUTES);

        /// <summary>
        /// Optional handle to the root object directory for the path name specified by the ObjectName member.
        /// If RootDirectory is NULL, ObjectName must point to a fully qualified object name that includes the full path to the target object.
        /// If RootDirectory is non-NULL, ObjectName specifies an object name relative to the RootDirectory directory.
        /// The RootDirectory handle can refer to a file system directory or an object directory in the object manager namespace.
        /// </summary>
        public IntPtr RootDirectory = IntPtr.Zero;

        /// <summary>
        /// Pointer to a Unicode string that contains the name of the object for which a handle is to be opened.
        /// This must either be a fully qualified object name, or a relative path name to the directory specified by the RootDirectory member.
        /// </summary>
        public UNICODE_STRING* ObjectName = default;

        /// <summary>
        /// Bitmask of flags that specify object handle attributes. This member can contain one or more of the flags in the following table (See MSDN)
        /// </summary>
        public uint Attributes = 0;

        /// <summary>
        /// Specifies a security descriptor (SECURITY_DESCRIPTOR) for the object when the object is created.
        /// If this member is NULL, the object will receive default security settings.
        /// </summary>
        public IntPtr SecurityDescriptor = IntPtr.Zero;

        /// <summary>
        /// Optional quality of service to be applied to the object when it is created.
        /// Used to indicate the security impersonation level and context tracking mode (dynamic or static).
        /// Currently, the InitializeObjectAttributes macro sets this member to NULL.
        /// </summary>
        public IntPtr SecurityQualityOfService = IntPtr.Zero;

        /// <summary/>
        public OBJECT_ATTRIBUTES() { }
    }

    /// <summary>
    /// Does this really need to be explained to you?
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UNICODE_STRING : IDisposable
    {
        /// <summary/>
        public ushort Length;

        /// <summary/>
        public ushort MaximumLength;

        /// <summary/>
        public IntPtr Buffer;

        /// <summary>
        /// Creates a native unicode string given a managed string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <remarks>Use the other overload and pin for temporary storage.</remarks>
        public UNICODE_STRING(string s)
        {
            Length = (ushort)(s.Length * 2);
            MaximumLength = (ushort)(Length + 2);
            Buffer = Marshal.StringToHGlobalUni(s);
        }

        /// <summary>
        /// Creates a native unicode string given a managed string.
        /// </summary>
        /// <param name="pointer">Address of the first character of the string.</param>
        /// <param name="length">Length of the string.</param>
        /// <remarks>Use the other overload and pin for temporary storage.</remarks>
        public UNICODE_STRING(char* pointer, int length)
        {
            Length = (ushort)(length * 2);
            MaximumLength = (ushort)(Length + 2);
            Buffer = (IntPtr)pointer;
        }

        /// <summary>
        /// Disposes of the current file name assigned to this Unicode String.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(Buffer);
            Buffer = IntPtr.Zero;
        }

        /// <summary>
        /// Returns a string with the contents
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                if (Buffer != IntPtr.Zero)
                    return Encoding.Unicode.GetString((byte*)Buffer, Length);

                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Returns a substring with the contents from the specified start index
        /// </summary>
        /// <param name="startIndex">The position to start the substring at</param>
        /// <returns>A substring with the contents</returns>
        public string Substring(int startIndex)
        {
            try
            {
                if (Buffer != IntPtr.Zero)
                    return Encoding.Unicode.GetString((byte*)(Buffer+startIndex*2), Length-startIndex*2);

                return "";
            }
            catch
            {
                return "";
            }
        }

    }

    /// <summary>
    /// Represents a 64-bit integer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct LARGE_INTEGER
    {
        /// <summary>
        /// The underlying 64-bit number.
        /// </summary>
        [FieldOffset(0)]
        public long QuadPart;

        /// <summary>
        /// The low part of the 64-bit number.
        /// </summary>
        [FieldOffset(0)]
        public int LowPart;

        /// <summary>
        /// The high part of the 64-bit number.
        /// </summary>
        [FieldOffset(4)]
        public uint HighPart;

        /// <summary>
        /// Converts the integer to a date and time [if used with file timestamps]
        /// </summary>
        public DateTime ToDateTime()
        {
            ulong high = HighPart;
            ulong low = (ulong)LowPart;
            long fileTime = (long)((high << 32) + low);
            return DateTime.FromFileTimeUtc(fileTime);
        }
    }

    /// <summary>The type of access to a file mapping object, which determines the page protection of the pages.</summary>
    [Flags]
    public enum FILE_MAP : uint
    {
        /// <summary>
        /// A read/write view of the file is mapped. The file mapping object must have been created with PAGE_READWRITE or
        /// PAGE_EXECUTE_READWRITE protection.
        /// <para>When used with MapViewOfFileEx, (FILE_MAP_WRITE | FILE_MAP_READ) and FILE_MAP_ALL_ACCESS are equivalent to FILE_MAP_WRITE.</para>
        /// </summary>
        FILE_MAP_WRITE = SECTION_MAP.SECTION_MAP_WRITE,

        /// <summary>
        /// A read-only view of the file is mapped. An attempt to write to the file view results in an access violation.
        /// <para>
        /// The file mapping object must have been created with PAGE_READONLY, PAGE_READWRITE, PAGE_EXECUTE_READ, or
        /// PAGE_EXECUTE_READWRITE protection.
        /// </para>
        /// </summary>
        FILE_MAP_READ = SECTION_MAP.SECTION_MAP_READ,

        /// <summary>
        /// A read/write view of the file is mapped. The file mapping object must have been created with PAGE_READWRITE or
        /// PAGE_EXECUTE_READWRITE protection.
        /// <para>When used with the MapViewOfFileEx function, FILE_MAP_ALL_ACCESS is equivalent to FILE_MAP_WRITE.</para>
        /// </summary>
        FILE_MAP_ALL_ACCESS = SECTION_MAP.SECTION_ALL_ACCESS,

        /// <summary>
        /// An executable view of the file is mapped (mapped memory can be run as code). The file mapping object must have been created
        /// with PAGE_EXECUTE_READ, PAGE_EXECUTE_WRITECOPY, or PAGE_EXECUTE_READWRITE protection.
        /// <para>
        /// Windows Server 2003 and Windows XP: This value is available starting with Windows XP with SP2 and Windows Server 2003 with SP1.
        /// </para>
        /// </summary>
        FILE_MAP_EXECUTE = SECTION_MAP.SECTION_MAP_EXECUTE_EXPLICIT,

        /// <summary>
        /// A copy-on-write view of the file is mapped. The file mapping object must have been created with PAGE_READONLY,
        /// PAGE_READ_EXECUTE, PAGE_WRITECOPY, PAGE_EXECUTE_WRITECOPY, PAGE_READWRITE, or PAGE_EXECUTE_READWRITE protection.
        /// <para>
        /// When a process writes to a copy-on-write page, the system copies the original page to a new page that is private to the
        /// process.The new page is backed by the paging file.The protection of the new page changes from copy-on-write to read/write.
        /// </para>
        /// <para>
        /// When copy-on-write access is specified, the system and process commit charge taken is for the entire view because the calling
        /// process can potentially write to every page in the view, making all pages private. The contents of the new page are never
        /// written back to the original file and are lost when the view is unmapped.
        /// </para>
        /// </summary>
        FILE_MAP_COPY = 0x00000001,

        /// <summary></summary>
        FILE_MAP_RESERVE = 0x80000000,

        /// <summary>
        /// Sets all the locations in the mapped file as invalid targets for CFG. This flag is similar to PAGE_TARGETS_INVALID. It is
        /// used along with the execute access right FILE_MAP_EXECUTE. Any indirect call to locations in those pages will fail CFG checks
        /// and the process will be terminated. The default behavior for executable pages allocated is to be marked valid call targets
        /// for CFG.
        /// </summary>
        FILE_MAP_TARGETS_INVALID = 0x40000000,

        /// <summary></summary>
        FILE_MAP_LARGE_PAGES = 0x20000000,
    }

    /// <summary>Section access rights.</summary>
    [Flags]
    public enum SECTION_MAP : uint
    {
        /// <summary>Query the section object for information about the section. Drivers should set this flag.</summary>
        SECTION_QUERY = 0x0001,

        /// <summary>Write views of the section.</summary>
        SECTION_MAP_WRITE = 0x0002,

        /// <summary>Read views of the section.</summary>
        SECTION_MAP_READ = 0x0004,

        /// <summary>Execute views of the section.</summary>
        SECTION_MAP_EXECUTE = 0x0008,

        /// <summary>Dynamically extend the size of the section.</summary>
        SECTION_EXTEND_SIZE = 0x0010,

        /// <summary>Undocumented.</summary>
        SECTION_MAP_EXECUTE_EXPLICIT = 0x0020,

        /// <summary>All of the previous flags combined with STANDARD_RIGHTS_REQUIRED.</summary>
        SECTION_ALL_ACCESS = 0x000F0000 | SECTION_QUERY | SECTION_MAP_WRITE | SECTION_MAP_READ | SECTION_MAP_EXECUTE | SECTION_EXTEND_SIZE,
    }
}
