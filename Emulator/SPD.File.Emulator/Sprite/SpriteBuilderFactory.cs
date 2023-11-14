using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using SPD.File.Emulator.Spd;
using SPD.File.Emulator.Spr;

namespace SPD.File.Emulator.Sprite;

public class SpriteBuilderFactory
{
    internal List<RouteGroupTuple> RouteGroupTuples = new();
    internal List<RouteFileTuple> RouteFileTuples = new();
    private readonly Logger _log;

    public SpriteBuilderFactory(Logger log)
    {
        _log = log;
    }

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

            string route = Route.GetRoute(redirectorFolder, group.Directory.FullPath);

            RouteGroupTuples.Add(new RouteGroupTuple()
            {
                Route = new Route(route),
                Files = group
            });
        }
    }

    public void AddFile(string file, string route)
    {
        _log.Info($"[SpdBuilderFactory] Added file {file} with route {route}");
        RouteFileTuples.Add(new RouteFileTuple { FilePath = file, Route = new Route(route) });
    }

    public bool TryCreateBuilder(string extension, out SpriteBuilder? builder)
    {
        builder = null;

        if (extension == ".spd")
            builder = new SpdBuilder(_log);
        else if (extension == ".spr")
            builder = new SprBuilder(_log);
        else
            return false;

        return true;
    }

    /// <summary>
    /// Tries to create an SPD from a given route.
    /// </summary>
    /// <param name="path">The file path/route to create SPD Builder for.</param>
    /// <param name="builder">The created builder.</param>
    /// <returns>True if a builder could be made, else false (if there are no files to modify this SPD).</returns>
    public bool TryCreateFromPath(string path, out SpriteBuilder? builder)
    {
        builder = default;
        var route = new Route(path);
        string routeExtension = Path.GetExtension(route.FullPath).ToLower();

        foreach (var group in RouteGroupTuples)
        {
            if (!route.Matches(group.Route.FullPath))
                continue;

            // Make builder if not made.
            if (builder == null)
                if (!TryCreateBuilder(routeExtension, out builder))
                    return false;

            // Add files to builder.
            string dir = group.Files.Directory.FullPath;
            foreach (string file in group.Files.Files)
            {
                builder?.AddOrReplaceFile(Path.Combine(dir, file));
            }
        }

        foreach (var group in RouteFileTuples)
        {
            if (!route.Matches(group.Route.FullPath))
                continue;

            // Make builder if not made.
            if (builder == null)
                if (!TryCreateBuilder(routeExtension, out builder))
                    return false;

            builder?.AddOrReplaceFile(group.FilePath);
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
    /// <summary>
    /// Route associated with this tuple.
    /// </summary>
    public Route Route;

    /// <summary>
    /// Path to the file bound by this route.
    /// </summary>
    public string FilePath;
}