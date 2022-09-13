using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.Utilities;
using static FileEmulationFramework.Lib.Utilities.MemoryMapFunctions;

namespace FileEmulationFramework.Lib.Memory;

/// <summary>
/// Represents a region of memory that can be mapped to.
/// Intended for use with <see cref="MemoryManager"/> to map multiple memory mapped files onto a single region,
/// when swapping out the used MMF to access further memory.
/// </summary>
public unsafe struct MemoryMappedRegion : IDisposable
{
    /// <summary>
    /// The region of memory mapped.
    /// </summary>
    public byte* MappedRegion { get; private set; } = (byte*)0;

    /// <summary>
    /// Size of the memory mapped.
    /// </summary>
    public int MemorySize { get; private set; }

    /// <summary>
    /// True if this region is mapped, (i.e. it is backed by a valid address), else false.
    /// </summary>
    public bool IsMapped => MappedRegion != (void*)0;

    /// <summary>
    /// True if disposed, else false.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    private MemoryMapFunctions _memoryMapFunctions = Instance;

    /// <summary>
    /// Represents a region of memory that can be mapped to.
    /// </summary>
    /// <param name="size">Size of the memory map that will be used.</param>
    internal MemoryMappedRegion(int size)
    {
        MemorySize = size;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Disposed)
            return;

        UnmapCurrent();
        MappedRegion = (byte*)0;
        Disposed = true;
    }

    /// <summary>
    /// Unmaps the currently mapped file and maps a new section to our committed memory.
    /// </summary>
    /// <param name="file">The memory mapped file to map.</param>
    public void Update(MemoryMappedFile file)
    {
        if (Disposed)
            ThrowHelpers.ObjectDisposed("This structure has already been disposed.");

        var fileHandle = file.SafeMemoryMappedFileHandle.DangerousGetHandle();
        UnmapCurrent();
        MappedRegion = (byte*)_memoryMapFunctions.MapViewOfFileEx(fileHandle, Native.FILE_MAP.FILE_MAP_WRITE, 0, 0, (uint)MemorySize, (IntPtr)0);
        if (MappedRegion == (void*)0)
            ThrowHelpers.Win32($"Failed to call MapViewOfFileEx with handle: {fileHandle}, size: {MemorySize} | W32 Error: {Marshal.GetLastWin32Error()}");
    }

    /// <summary>
    /// Unmaps the currently mapped file.
    /// </summary>
    private void UnmapCurrent()
    {
        if (MappedRegion != (void*)0 && !_memoryMapFunctions.UnmapViewOfFileEx((IntPtr)MappedRegion, 1))
            ThrowHelpers.Win32($"Failed to call UnmapViewOfFileEx with {(long)MappedRegion:X}, {1} | W32 Error:  {Marshal.GetLastWin32Error()}");
    }
}