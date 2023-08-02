using PAK.Stream.Emulator.Interfaces.Structures.IO;
using Strim = System.IO.Stream;

namespace PAK.Stream.Emulator.Interfaces;

/// <summary>
/// APIs exposed by PAK Emulator.
/// </summary>
public interface IPakEmulator
{
    /// <summary>
    /// Tries to create an emulated PAK file using an PAK file embedded inside another file as source.
    /// </summary>
    /// <param name="sourcePath">Path to the file from which the data will be sourced.</param>
    /// <param name="offset">Offset in the file where the PAK starts.</param>
    /// <param name="route">The route of the emulated file.</param>
    /// <param name="destinationPath">Path to where the emulated file should be put.</param>
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath);

    /// <summary>
    /// Invalidates a file, i.e. unregisters it, will be recreated on next access.
    /// </summary>
    /// <param name="pakPath">Path of the PAK file.</param>
    public void InvalidateFile(string pakPath);
    
    /// <summary>
    /// Gets the list of input files from other mods fed into the emulator.
    /// </summary>
    public RouteGroupTuple[] GetEmulatorInput();

    /// <summary>
    /// Gets an entry from a pak file
    /// </summary>
    /// <param name="pak">A stream containing the pak file positioned at the beginning of it</param>
    /// <param name="entryPath">The path to the desired entry in the file (e.g. field/script/field.bf)</param>
    /// <returns>The data of the desired entry or null if it could not be found in the pak</returns>
    public ReadOnlyMemory<byte>? GetEntry(Strim pak, string entryPath);

    /// <summary>
    /// Adds a new file to be injected into emulated paks
    /// </summary>
    /// <param name="file">The path to the file to add</param>
    /// <param name="route">The route the file is in</param>
    /// <param name="inPakPath">The path in the pak the file will take</param>
    public void AddFile(string file, string route, string inPakPath);

    /// <summary>
    /// Adds a directory to PAK Emulator so it's like the files were in FEmulator\PAK
    /// </summary>
    /// <param name="dir">The directory to add the files from</param>
    public void AddDirectory(string dir);
}