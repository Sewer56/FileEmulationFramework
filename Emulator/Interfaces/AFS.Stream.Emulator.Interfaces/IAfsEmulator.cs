namespace AFS.Stream.Emulator.Interfaces;

/// <summary>
/// Registers files and redirector-style directory roots with the AFS emulator.
/// </summary>
public interface IAfsEmulator
{
    /// <summary>
    /// Registers a source file for an index in matching AFS archives created after this call.
    /// </summary>
    /// <param name="file">Path to the source file whose bytes replace the archive entry.</param>
    /// <param name="route">Route used to select matching AFS archive paths.</param>
    /// <param name="index">Zero-based AFS entry index, from 0 through 65535.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or greater than 65535.</exception>
    public void AddFile(string file, string route, int index);

    /// <summary>
    /// Registers a directory root as though its contents were under a mod's <c>FEmulator/AFS</c> folder.
    /// Registrations apply only to archives first emulated after this call.
    /// </summary>
    /// <param name="dir">Path to the redirector-style directory root.</param>
    public void AddDirectory(string dir);
}
