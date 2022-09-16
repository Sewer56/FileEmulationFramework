namespace FileEmulationFramework.Lib;

/// <summary>
/// Represents the current route.
/// </summary>
public struct Route
{
    /// <summary>
    /// Full path of the currently being resolved item.
    /// </summary>
    public string FullPath { get; private set; }

    /// <summary>
    /// Creates a route from a given provided full path.
    /// </summary>
    /// <param name="fullPath">Full path to the item.</param>
    public Route(string fullPath)
    {
        FullPath = fullPath;
    }

    /// <summary>
    /// Merges a file path into the existing route, by matching folder names from the end.
    /// 
    /// <br/><br/>
    /// Example:
    /// <br/><br/>
    /// Data/BGM/VOICE.AFS <br/>
    /// and 'FEmulator/AFS/VOICE.AFS/00000.adx <br/> <br/>
    ///
    /// would merge into 'Data/BGM/VOICE.AFS/00000.adx'
    /// </summary>
    /// <param name="otherPath">Path to merge into existing path.</param>
    /// <returns>The merged path.</returns>
    public Route Merge(string otherPath)
    {
        // This might be probably even more optimisable with HW intrinsics
        // all the way through but don't have time to invest.
        
        // Currently this executes in around 49ns on my 4790k and does no allocation outside of creation
        // of final spring.
        var lastDirectory = Path.GetFileName(FullPath.AsSpan());
        var offsetOfLastDir = otherPath.AsSpan().IndexOf(lastDirectory, StringComparison.OrdinalIgnoreCase);
        if (offsetOfLastDir == -1) 
            return this;

        // Found it.
        var path = string.Concat(FullPath.AsSpan(0, FullPath.Length), otherPath.AsSpan(offsetOfLastDir + lastDirectory.Length));
        return new Route(path);
    }

    /// <summary>
    /// Checks if the given path matches this route.
    /// </summary>
    /// <param name="filePath">The route/file to test.</param>
    /// <returns>True if the route matches, else false.</returns>
    public readonly bool Matches(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;
        
        return FullPath.Contains(filePath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a route given full path of a file and the folder where a given emulator's files are contained.
    /// </summary>
    /// <param name="baseFolder">The full path to the folder where the emulator's files are contained for a user mod.</param>
    /// <param name="fullPath">The full path to the file to get route for.</param>
    public static string GetRoute(string baseFolder, string fullPath)
    {
        if (baseFolder.Length + 1 > fullPath.Length)
            return "";
        
        return fullPath.Substring(baseFolder.Length + 1);
    }

    /// <summary>
    /// True if this route has no value, else false.
    /// </summary>
    public bool IsEmpty() => string.IsNullOrEmpty(FullPath);
}