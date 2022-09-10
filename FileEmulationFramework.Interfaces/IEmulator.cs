namespace FileEmulationFramework.Interfaces;

/// <summary>
/// Common interface implemented by emulators.
/// </summary>
public interface IEmulator
{
    /// <summary>
    /// Verifies whether this file can be accepted by this emulator by checking extension and header.
    /// If it cannot, return false.
    /// If it can, build the file and cache it using file path.
    /// </summary>
    /// <param name="handle">Handle to the file in question. Assume it has been already opened.</param>
    /// <param name="filepath">Full path to the file. Use this as cache key.</param>
    /// <returns>True if the file will be used with this emulator, else false.</returns>
    /// <remarks>
    ///     In your emulator, you should first filter by extension and then by header to verify this is the file you want.
    ///     Created files should be cached. Subsequent requests with same file path should reuse the cached file.
    /// </remarks>
    public bool TryCreateFile(IntPtr handle, string filepath);

    /// <summary>
    /// Gets the file size of a file with a given handle.
    /// This function will only be invoked if you have accepted the file with <see cref="TryCreateFile"/>.
    /// </summary>
    /// <param name="handle">The handle of the file.</param>
    /// <returns>Size of the emulated file.</returns>
    /// <remarks>This function will only be invoked if you have accepted the file with <see cref="TryCreateFile"/>.</remarks>
    public long GetFileSize(IntPtr handle);

    /// <summary>
    /// Reads a specified number of bytes.
    /// </summary>
    /// <param name="handle">Handle to the file.</param>
    /// <param name="buffer">Pointer to the address that will receive the data.</param>
    /// <param name="length">Length of the data to be received.</param>
    /// <param name="offset">Offset of the data to be received.</param>
    /// <param name="numReadBytes">Number of bytes read by the function call.</param>
    /// <returns>True if the operation succeeded, else false.</returns>
    public unsafe bool ReadData(IntPtr handle, byte* buffer, uint length, long offset, out int numReadBytes);
}