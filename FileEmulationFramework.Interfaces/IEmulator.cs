namespace FileEmulationFramework.Interfaces;

/// <summary>
/// Common interface implemented by emulators.
/// </summary>
public interface IEmulator
{
    /// <summary>
    /// Relative to the folder containing emulator's data inside the user folder.
    /// Should be of the format 'FEmulator{Path.DirectorySeparatorChar}EMULATOR{Path.DirectorySeparatorChar}' e.g. 'FEmulator{Path.DirectorySeparatorChar}AFS{Path.DirectorySeparatorChar}'
    /// </summary>
    public string Folder { get; }

    /// <summary>
    /// Verifies whether this file can be accepted by this emulator by checking extension and header.
    /// If it cannot, return false.
    /// If it can, build the file and cache it using file path.
    /// </summary>
    /// <param name="handle">Handle to the file in question. Assume it has been already opened.</param>
    /// <param name="filepath">Full path to the file. Use this as cache key and determine route from this using Route struct.</param>
    /// <param name="route">The current route for the file being resolved, as defined in 'Routing' on the wiki.</param>
    /// <returns>True if the file will be used with this emulator, else false.</returns>
    /// <remarks>
    ///     In your emulator, you should first filter by extension and then by header to verify this is the file you want.
    ///     Created files should be cached. Subsequent requests with same file path should reuse the cached file.
    ///
    ///     You should reset the read pointer to 0.
    /// </remarks>
    public bool TryCreateFile(IntPtr handle, string filepath, string route);

    /// <summary>
    /// Gets the file size of a file with a given handle.
    /// This function will only be invoked if you have accepted the file with <see cref="TryCreateFile"/>.
    /// </summary>
    /// <param name="handle">The handle of the file.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    /// <returns>Size of the emulated file.</returns>
    /// <remarks>This function will only be invoked if you have accepted the file with <see cref="TryCreateFile"/>.</remarks>
    public long GetFileSize(IntPtr handle, IFileInformation info);

    /// <summary>
    /// Reads a specified number of bytes.
    /// </summary>
    /// <param name="handle">Handle to the file.</param>
    /// <param name="buffer">Pointer to the address that will receive the data.</param>
    /// <param name="length">Length of the data to be received.</param>
    /// <param name="offset">Offset of the data to be received.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    /// <param name="numReadBytes">Number of bytes read by the function call.</param>
    /// <returns>True if the operation succeeded, else false. Return true if at least 1 byte was read.</returns>
    public unsafe bool ReadData(IntPtr handle, byte* buffer, uint length, long offset, IFileInformation info, out int numReadBytes);
    
    /// <summary>
    /// Called when a handle to the given file is closed..
    /// </summary>
    /// <param name="handle">The handle of the file.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    /// <remarks>This function will only be invoked if you have accepted the file with <see cref="TryCreateFile"/>.</remarks>
    public void CloseHandle(IntPtr handle, IFileInformation info);
}