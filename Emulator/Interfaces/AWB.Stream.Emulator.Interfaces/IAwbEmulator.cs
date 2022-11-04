namespace AWB.Stream.Emulator.Interfaces;

/// <summary>
/// APIs exposed by AWB Emulator.
/// </summary>
public interface IAwbEmulator
{
    /// <summary>
    /// Tries to create an emulated AWB file using an AWB file embedded inside another file as source.
    /// </summary>
    /// <param name="sourcePath">Path to the file from which the data will be sourced.</param>
    /// <param name="offset">Offset in the file where the AWB starts.</param>
    /// <param name="route">The route of the emulated file.</param>
    /// <param name="destinationPath">Path to where the emulated file should be put.</param>
    public bool TryCreateFromFileSlice(string sourcePath, long offset, string route, string destinationPath);
}