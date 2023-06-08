using FileEmulationFramework.Lib.Utilities;
using System.Collections.Concurrent;

namespace FileEmulationFramework.Lib.IO;

/// <summary>
/// An abstraction used to acquiring slices of files stored externally on disk.
/// Represents a slice of a file that's stored externally on disk.
/// </summary>
public class FileSlice
{
    /// <summary>
    /// Caches existing open handles to the file.
    /// Used for fast subsequent access of files; we keep the handles open.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IntPtr> HandleCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Offset of the data to get from the file.
    /// </summary>
    public long Offset { get; private set; }

    /// <summary>
    /// End of the slice [exclusive]. (Offset + Length)
    /// i.e. Offset of first byte after the last byte of this slice.
    /// </summary>
    public long End => Offset + Length;

    /// <summary>
    /// Length of the data to get from the file.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Path to the file on the hard disk.
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// Gets the handle used to access the file.
    /// </summary>
    public IntPtr Handle { get; private set; }

    /// <summary>
    /// Creates a slice of an external file.
    /// For internal use.
    /// </summary>
    /// <param name="offset">Offset of the file.</param>
    /// <param name="length">Length of the file.</param>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="handle">Handle of this file.</param>
    private FileSlice(long offset, int length, string filePath, IntPtr handle)
    {
        Offset = offset;
        Length = length;
        FilePath = filePath;
        Handle = handle;
    }

    /// <summary>
    /// Creates a slice of an external file.
    /// </summary>
    /// <param name="offset">Offset of the file.</param>
    /// <param name="length">Length of the file.</param>
    /// <param name="filePath">Path to the file.</param>
    public FileSlice(long offset, int length, string filePath)
    {
        Offset = offset;
        Length = length;
        FilePath = filePath;
        SetHandle();
    }

    /// <summary>
    /// Creates a slice of an external file.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    public FileSlice(string filePath)
    {
        Offset = 0;
        FilePath = filePath;
        SetHandle();
        Length = (int)new FileInfo(filePath).Length;
    }

    private void SetHandle()
    {
        if (HandleCache.ContainsKey(FilePath))
        {
            Handle = HandleCache[FilePath];
        }
        else
        {
            Handle = Native.CreateFileW(FilePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            HandleCache[FilePath] = Handle;
        }
    }

    /// <summary>
    /// Reads the data from the hard disk, returning the number of read bytes into the buffer.
    /// </summary>
    public unsafe int GetData(Span<byte> buffer)
    {
        buffer = buffer.Slice(0, Length);
        fixed (byte* buf = buffer)
        {
            // TODO: We might need to add support for asynchronous handles at some point.
            if (!NativeExtensions.TryReadFile(Handle, Offset, buf, (uint)Length, out _))
                ThrowHelpers.Win32("Failed to read all bytes from requested file.\n" +
                                            $"FileSlice: {this}");
        }

        return Length;
    }

    /// <summary>
    /// Reads the data from the hard disk, returning the number of read bytes into the buffer.
    /// </summary>
    public int GetData(out byte[] bytes)
    {
        bytes = GC.AllocateUninitializedArray<byte>(Length);
        return GetData(bytes);
    }

    /// <summary>
    /// Gets a <see cref="FileSlice"/> that corresponds to a slice of this <see cref="FileSlice"/>
    /// instance.
    /// </summary>
    /// <param name="offset">Offset of the slice relative to the current <see cref="Offset"/>.</param>
    /// <param name="length">Length of the slice starting from the current <see cref="Offset"/>.</param>
    /// <returns></returns>
    public FileSlice Slice(long offset, int length)
    {
        // Error checking, just in case.
        var finalOffset = Offset + offset;
        var maxAllowedOffset = End;
        if (finalOffset < Offset || finalOffset >= maxAllowedOffset)
            ThrowHelpers.Argument("Requested offset is out of range. Is neither negative or beyond end of file.");

        var requestedEndOfFile = finalOffset + length;
        if (requestedEndOfFile < finalOffset || requestedEndOfFile > maxAllowedOffset)
            ThrowHelpers.Argument("Requested length is out of range. Is neither negative or will read beyond end of file.");

        return new(finalOffset, length, FilePath);
    }

    /// <summary>
    /// Gets a <see cref="FileSlice"/> that corresponds to a slice of this <see cref="FileSlice"/>.
    /// The length represents the maximum length of the slice. If the slice goes out of file range, the 
    /// length will be capped at the maximum possible value.
    /// </summary>
    /// <param name="offset">Offset of the slice relative to the current <see cref="Offset"/>.</param>
    /// <param name="length">Length of the slice starting from the current <see cref="Offset"/>.</param>
    /// <returns></returns>
    public FileSlice SliceUpTo(long offset, int length)
    {
        // Error checking, just in case.
        var finalOffset = Offset + offset;
        var maxAllowedOffset = End;
        if (finalOffset < Offset || finalOffset >= maxAllowedOffset)
            ThrowHelpers.Argument("Requested offset is out of range. Is neither negative or beyond end of file.");

        var requestedEndOfFile = finalOffset + length;
        if (requestedEndOfFile < finalOffset)
            ThrowHelpers.Argument("Requested length is out of range. It is negative.");

        var endOfFile = Offset + Length;
        if (requestedEndOfFile > endOfFile)
            length -= (int)(requestedEndOfFile - endOfFile);

        return new(finalOffset, length, FilePath);
    }

    /// <summary>
    /// Converts the current file slice to an <see cref="OffsetRange"/>.
    /// </summary>
    public OffsetRange ToOffsetRange() => OffsetRange.FromStartAndLength(Offset, Length);

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FileSlice {{ Path: {FilePath}, Offset: {Offset}, Length: {Length}, Handle: {Handle} }}";
    }

    /// <summary>
    /// Tries to merge two file slices into one.
    /// </summary>
    /// <param name="first">First slice.</param>
    /// <param name="second">Second slice.</param>
    /// <param name="result">Result slice.</param>
    /// <remarks>
    ///     Slices will only be merged if there is no gap between first and second slice.
    ///     Does not support overlapping slices.
    /// </remarks>
    public static bool TryMerge(FileSlice first, FileSlice second, out FileSlice? result)
    {
        // Slices refer to different files.
        if (first.Handle != second.Handle)
        {
            result = null;
            return false;
        }

        // Sort by memory address.
        var slices = SortSlices(first, second);
        if (slices.first.End != slices.second.Offset)
        {
            result = null;
            return false;
        }

        // Check for overflow.
        var combinedLength = (long)slices.first.Length + slices.second.Length;
        if (combinedLength > int.MaxValue)
        {
            result = null;
            return false;
        }

        result = new FileSlice(slices.first.Offset, (int)combinedLength, first.FilePath, first.Handle);
        return true;
    }

    private static (FileSlice first, FileSlice second) SortSlices(FileSlice first, FileSlice second)
    {
        return first.Offset < second.Offset ? (first, second) : (second, first);
    }
}