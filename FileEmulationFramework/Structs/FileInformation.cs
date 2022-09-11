using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework.Structs;

/// <summary>
/// Contains current information about a given file being handled.
/// </summary>
public class FileInformation
{
    /// <summary>
    /// Contains the absolute file path to the file.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Current read pointer for the file.
    /// </summary>
    public long FilePointer { get; set; }

    /// <summary>
    /// The emulator associated with this file.
    /// </summary>
    public IEmulator Emulator { get; private set; }

    public FileInformation(string filePath, long filePointer, IEmulator emulator)
    {
        FilePath = filePath;
        FilePointer = filePointer;
        Emulator = emulator;
    }
}