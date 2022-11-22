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
    /// <param name="filepath">Full path to the file. Use this as cache key and determine route from this using Route struct.</param>
    /// <param name="route">The current route for the file being resolved, as defined in 'Routing' on the wiki.</param>
    /// <param name="result">The created emulated file to be used by the framework.</param>
    /// <returns>True if the file will be used with this emulator, else false.</returns>
    /// <remarks>
    ///     In your emulator, you should first filter by extension and then by header to verify this is the file you want.
    ///     Created files should be cached. Subsequent requests with same file path should reuse the cached file.
    /// 
    ///     You should reset the read pointer to 0.
    /// </remarks>
    public bool TryCreateFile(IntPtr handle, string filepath, string route, out IEmulatedFile result);
}