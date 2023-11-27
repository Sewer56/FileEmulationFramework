using BMD.File.Emulator.Interfaces.Structures.IO;

namespace BMD.File.Emulator.Interfaces;

public interface IBmdEmulator
{
    /// <summary>
    /// Tries to create an emulated BMD file by compiling any applicable flow files using the input as the basee
    /// </summary>
    /// <param name="sourcePath">Path to the bmd file to use as a base.</param>
    /// <param name="route">The route of the emulated bmd file.</param>
    /// <param name="destinationPath">Path to where the emulated bmd file should be put.</param>
    public bool TryCreateFromBmd(string sourcePath, string route, string destinationPath);

    /// <summary>
    /// Invalidates a file, i.e. unregisters it, will be recreated on next access.
    /// </summary>
    /// <param name="bmdPath">Path of the BMD file.</param>
    public void InvalidateFile(string bmdPath);

    /// <summary>
    /// Gets the list of input files from other mods fed into the emulator.
    /// </summary>
    public RouteFileTuple[] GetEmulatorInput();

    /// <summary>
    /// Registers an already compiled BMD as an emulated one
    /// </summary>
    /// <param name="sourcePath">The path to the bmd file to registeer</param>
    /// <param name="destinationPath">The path where the emulated bmd file should be put</param>
    public void RegisterBmd(string sourcePath, string destinationPath);

    /// <summary>
    /// Adds a new file to be used when compiling bmds
    /// </summary>
    /// <param name="file">The path to the file to add</param>
    /// <param name="route">The route the file is in</param>
    public void AddFile(string file, string route);

    /// <summary>
    /// Adds a directory to BMD Emulator so it's like the files were in FEmulator\BMD
    /// </summary>
    /// <param name="dir">The directory to add the files from</param>
    public void AddDirectory(string dir);
}
