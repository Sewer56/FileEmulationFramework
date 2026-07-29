namespace ONE.Heroes.Stream.Emulator.Interfaces;

/// <summary>
/// Registers files and redirector-style directory roots with the Sonic Heroes ONE emulator.
/// </summary>
public interface IOneEmulator
{
    /// <summary>
    /// Registers a source file for matching ONE archives created after this call.
    /// The source file name becomes the case-sensitive archive entry name; a <c>.prs</c> suffix marks
    /// pre-compressed data and a <c>.del</c> suffix removes the entry named by the remaining file name.
    /// </summary>
    /// <param name="file">Path to the source file.</param>
    /// <param name="route">Route used to select matching ONE archive paths.</param>
    public void AddFile(string file, string route);

    /// <summary>
    /// Registers a directory root as though its contents were under a mod's <c>FEmulator/ONE</c> folder.
    /// Registrations apply only to archives first emulated after this call.
    /// </summary>
    /// <param name="dir">Path to the redirector-style directory root.</param>
    public void AddDirectory(string dir);
}
