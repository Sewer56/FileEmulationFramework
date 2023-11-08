using SPD.File.Emulator.Interfaces.Structures.IO;

namespace SPD.File.Emulator.Interfaces;

/// <summary>
/// APIs exposed by Spd Emulator.
/// </summary>
public interface ISpdEmulator
{
    /// <summary>
    /// Tries to create an emulated SPD file using an SPD file embedded inside another file as source.
    /// </summary>
    /// <param name="sourcePath">Path to the file from which the data will be sourced.</param>
    /// <param name="offset">Offset in the file where the SPD starts.</param>
    /// <param name="route">The route of the emulated file.</param>
    /// <param name="destinationPath">Path to where the emulated file should be put.</param>
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath);

    /// <summary>
    /// Tries to create an emulated SPD file by compiling any applicable flow files using the input as the basee
    /// </summary>
    /// <param name="sourcePath">Path to the spd file to use as a base.</param>
    /// <param name="route">The route of the emulated spd file.</param>
    /// <param name="destinationPath">Path to where the emulated spd file should be put.</param>
    public bool TryCreateFromSpd(string sourcePath, string route, string destinationPath);

    /// <summary>
    /// Invalidates a file, i.e. unregisters it, will be recreated on next access.
    /// </summary>
    /// <param name="spdPath">Path of the SPD file.</param>
    public void InvalidateFile(string spdPath);

    /// <summary>
    /// Gets the list of input files from other mods fed into the emulator.
    /// </summary>
    public RouteGroupTuple[] GetEmulatorInput();

    /// <summary>
    /// Registers an already merged SPD as an emulated one
    /// </summary>
    /// <param name="sourcePath">The path to the spd file to registeer</param>
    /// <param name="destinationPath">The path where the emulated spd file should be put</param>
    public void RegisterSpd(string sourcePath, string destinationPath);

    /// <summary>
    /// Adds a new file to be injected into emulated spds
    /// </summary>
    /// <param name="file">The path to the file to add</param>
    /// <param name="route">The route the file is in</param>
    public void AddFile(string file, string route);

    /// <summary>
    /// Adds a directory to SPD Emulator so it's like the files were in FEmulator\SPD
    /// </summary>
    /// <param name="dir">The directory to add the files from</param>
    public void AddDirectory(string dir);
}