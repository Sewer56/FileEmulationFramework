namespace FileEmulationFramework.Interfaces;

/// <summary>
/// Contains current information about the current file being handled.
/// </summary>
public interface IFileInformation
{
    /// <summary>
    /// Contains the absolute file path to the file.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Current read offset for the file.
    /// </summary>
    long FileOffset { get; }

    /// <summary>
    /// The emulated file.
    /// </summary>
    IEmulatedFile File { get; }
}