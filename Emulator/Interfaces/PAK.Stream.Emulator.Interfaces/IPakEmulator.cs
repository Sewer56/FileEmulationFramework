using PAK.Stream.Emulator.Interfaces.Structures.IO;

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
}