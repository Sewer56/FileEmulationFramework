namespace PAK.Stream.Emulator.Interfaces.Structures.IO;

/// <summary>
/// Represents information tied to an individual directory.
/// </summary>
public struct DirectoryInformation
{
    /// <summary>
    /// Full path to the directory.
    /// </summary>
    public string FullPath;

    /// <summary>
    /// Last time this directory was modified.
    /// </summary>
    public DateTime LastWriteTime;
}

/// <summary>
/// Groups a single directory and a list of files associated with it.
/// </summary>
public class DirectoryFilesGroup
{
    /// <summary>
    /// The directory in question.
    /// </summary>
    public DirectoryInformation Directory;

    /// <summary>
    /// The relative file paths tied to this directory.
    /// </summary>
    public string[] Files = Array.Empty<string>();
}

/// <summary>
/// A tuple representing a group and a route.
/// </summary>
public struct RouteGroupTuple
{
    /// <summary>
    /// Route associated with this tuple.
    /// </summary>
    public string Route;

    /// <summary>
    /// Files bound by this route.
    /// </summary>
    public DirectoryFilesGroup Files;
}