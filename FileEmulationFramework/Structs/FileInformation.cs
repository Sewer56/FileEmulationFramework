using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework.Structs;

/// <summary>
/// Contains current information about a given file being handled.
/// </summary>
public class FileInformation : IFileInformation
{
    /// <summary>
    /// Contains the absolute file path to the file.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Current read offset for the file.
    /// </summary>
    public long FileOffset { get; set; }

    /// <summary>
    /// The emulator associated with this file.
    /// </summary>
    public IEmulator Emulator { get; private set; }

    /// <summary/>
    /// <param name="filePath">Path to the file in question.</param>
    /// <param name="fileOffset">Current read offset from start of file.</param>
    /// <param name="emulator">Emulator associated with this info.</param>
    public FileInformation(string filePath, long fileOffset, IEmulator emulator)
    {
        FilePath = filePath;
        FileOffset = fileOffset;
        Emulator = emulator;
    }
}