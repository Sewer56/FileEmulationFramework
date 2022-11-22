using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework.Structs;

/// <summary>
/// Contains current information about a given file being handled.
/// </summary>
public class FileInformation : IFileInformation
{
    /// <inheritdoc/>
    public string FilePath { get; set; }

    /// <inheritdoc/>
    public long FileOffset { get; set; }

    /// <inheritdoc/>
    public IEmulatedFile File { get; private set; }

    /// <summary/>
    /// <param name="filePath">Path to the file in question.</param>
    /// <param name="fileOffset">Current read offset from start of file.</param>
    /// <param name="file">The emulated file returned from an emulator.</param>
    public FileInformation(string filePath, long fileOffset, IEmulatedFile file)
    {
        FilePath = filePath;
        FileOffset = fileOffset;
        File = file;
    }
}