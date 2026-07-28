using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.IO;

namespace AFS.Stream.Emulator.Afs;

public class AfsBuilderFactory
{
    private readonly List<RouteGroupTuple> _routeGroupTuples = new();
    private readonly List<RouteFileTuple> _routeFileTuples = new();

    /// <summary>
    /// Adds all available routes from folders.
    /// </summary>
    /// <param name="redirectorFolder">Folder containing the redirector's files.</param>
    public void AddFromFolders(string redirectorFolder)
    {
        // Get contents.
        WindowsDirectorySearcher.GetDirectoryContentsRecursiveGrouped(redirectorFolder, out var groups);

        // Find matching folders.
        foreach (var group in groups)
        {
            if (group.Files.Length <= 0)
                continue;

            var route = Route.GetRoute(redirectorFolder, group.Directory.FullPath);

            _routeGroupTuples.Add(new RouteGroupTuple()
            {
                Route = new Route(route),
                Files = group
            });
        }
    }

    /// <summary>
    /// Adds an indexed source file for archives matching a route.
    /// </summary>
    /// <param name="file">Path to the source file.</param>
    /// <param name="route">Route used to match archive paths.</param>
    /// <param name="index">Zero-based archive entry index.</param>
    internal void AddFile(string file, string route, int index)
    {
        _routeFileTuples.Add(new RouteFileTuple
        {
            Route = new Route(route),
            FilePath = file,
            Index = index
        });
    }

    /// <summary>
    /// Tries to create an AFS from a given route.
    /// </summary>
    /// <param name="path">The file path/route to create AFS Builder for.</param>
    /// <param name="builder">The created builder.</param>
    /// <returns>True if a builder could be made, else false (if there are no files to modify this AFS).</returns>
    public bool TryCreateFromPath(string path, out AfsBuilder? builder)
    {
        builder = default;
        var route = new Route(path);
        foreach (var group in _routeGroupTuples)
        {
            if (!route.Matches(group.Route.FullPath))
                continue;

            // Make builder if not made.
            builder ??= new AfsBuilder();

            // Add files to builder.
            var dir = group.Files.Directory.FullPath;
            foreach (var file in group.Files.Files)
                builder.AddOrReplaceFile(Path.Combine(dir, file));
        }

        foreach (var file in _routeFileTuples)
        {
            if (!route.Matches(file.Route.FullPath))
                continue;

            builder ??= new AfsBuilder();
            builder.AddOrReplaceFile(file.Index, file.FilePath);
        }

        return builder != null;
    }
}

internal struct RouteGroupTuple
{
    /// <summary>
    /// Route associated with this tuple.
    /// </summary>
    public Route Route;

    /// <summary>
    /// Files bound by this route.
    /// </summary>
    public DirectoryFilesGroup Files;
}

internal struct RouteFileTuple
{
    public Route Route;
    public string FilePath;
    public int Index;
}
