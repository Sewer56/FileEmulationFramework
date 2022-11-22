using System.IO.MemoryMappedFiles;

namespace FileEmulationFramework.Lib.Memory;

/// <summary>
/// A custom implementation of a Memory Manager that allows access to large amounts of memory using Memory Mapped Files.
/// Basically we are doing memory paging, but with memory mapped files.
/// </summary>
/// <remarks>Ideally would like to use AWE (Windowing Extensions), but those don't flush to page file, risking overloading user's RAM.</remarks>
public sealed class MemoryManager : IDisposable
{
    private const int WindowsAllocationGranularity = 64 * 1024;
    private List<MemoryMappedFile> _files = new List<MemoryMappedFile>();

    /// <summary>
    /// The granularity at which memory mapped files inside are allocated.
    /// </summary>
    public int AllocationGranularity { get; private set; }

    /// <summary>
    /// Returns the total allocated size in the manager.
    /// </summary>
    public long Size => _files.Count * AllocationGranularity;

    /// <summary>
    /// Returns the total number of pages allocated.
    /// </summary>
    public int PageCount => _files.Count;

    /// <summary>
    /// Creates a memory manager.
    /// </summary>
    /// <param name="allocationGranularity">Amount of bytes in each underlying memory mapped file.</param>
    public MemoryManager(int allocationGranularity)
    {
        if (allocationGranularity % WindowsAllocationGranularity != 0)
            throw new ArgumentException($"The allocation granularity must be a multiple of {WindowsAllocationGranularity} and greater than zero");

        AllocationGranularity = allocationGranularity;
    }

    /// <inheritdoc />
    ~MemoryManager() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var file in _files)
            file.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allocates a certain given amount of bytes into the memory mapped file.
    /// </summary>
    /// <param name="amountOfBytes">The amount of bytes to grow by. This is rounded up to <see cref="AllocationGranularity"/>.</param>
    public void Allocate(long amountOfBytes)
    {
        long numAllocations = (amountOfBytes - 1) / AllocationGranularity + 1;
        for (int x = 0; x < numAllocations; x++)
            _files.Add(MemoryMappedFile.CreateNew(null, AllocationGranularity, MemoryMappedFileAccess.ReadWriteExecute));
    }

    /// <summary>
    /// Gets the memory mapped file tied to a specific offset.
    /// </summary>
    /// <param name="offset">Declares which memory mapped file to obtain.</param>
    /// <param name="byteOffset">Offset into the returned memory mapped file.</param>
    /// <param name="bytesAvailable">Number of bytes available in this memory mapped file.</param>
    public MemoryMappedFile GetMappedFile(long offset, out int byteOffset, out int bytesAvailable)
    {
        var mapIndex = GetMappedFileIndex(offset, out byteOffset, out bytesAvailable);
        return _files[(int)mapIndex];
    }

    /// <summary>
    /// Gets the memory mapped file tied to a specific offset.
    /// Allocates if there is not sufficient data.
    /// </summary>
    /// <param name="offset">Declares which memory mapped file to obtain.</param>
    /// <param name="byteOffset">Offset into the returned memory mapped file.</param>
    /// <param name="bytesAvailable">Number of bytes available in this memory mapped file.</param>
    public MemoryMappedFile GetMappedFileWithAlloc(long offset, out int byteOffset, out int bytesAvailable)
    {
        var mapIndex = GetMappedFileIndex(offset, out byteOffset, out bytesAvailable);
        var neededAllocations = mapIndex - _files.Count + 1;
        if (neededAllocations > 0)
            Allocate(neededAllocations * AllocationGranularity);

        return _files[(int)mapIndex];
    }

    /// <summary>
    /// Creates a memory mapped region that can be used to access the contents of a single memory mapped file at any given time.
    /// </summary>
    public MemoryMappedRegion CreateMappedRegion() => new MemoryMappedRegion(AllocationGranularity);

    private long GetMappedFileIndex(long offset, out int byteOffset, out int bytesAvailable)
    {
        var mapIndex = offset / AllocationGranularity;
        var firstByte = mapIndex * AllocationGranularity;
        var lastByte = firstByte + AllocationGranularity;
        byteOffset = (int)(offset - firstByte);
        bytesAvailable = (int)(lastByte - offset);
        return mapIndex;
    }
}