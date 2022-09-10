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
    /// Appends a path onto the current path.
    /// </summary>
    /// <param name="child">Name of the file held by the child.</param>
    public Route Append(string child)
    {
        return new Route(FullPath + $"{Path.DirectorySeparatorChar}{child}");
    }

    /// <summary>
    /// Checks if the given path matches this route.
    /// </summary>
    /// <param name="filePath">The route/file to test.</param>
    /// <returns>True if the route matches, else false.</returns>
    public bool Matches(string filePath) => FullPath.Contains(filePath, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a route given full path of a file and the folder where a given emulator's files are contained.
    /// </summary>
    /// <param name="baseFolder">The full path to the folder where the emulator's files are contained for a user mod.</param>
    /// <param name="fullPath">The full path to the file to get route for.</param>
    public static string GetRoute(string baseFolder, string fullPath)
    {
        return fullPath.Substring(baseFolder.Length + 1);
    }
}