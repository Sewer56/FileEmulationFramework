using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.IO;
using System.Text.RegularExpressions;

namespace BF.File.Emulator.Bf
{

    internal class BfBuilderFactory
    {

        internal List<RouteGroupTuple> RouteFileTuples = new();

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
                foreach (var file in group.Files)
                {
                    if (!file.EndsWith(".flow", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var filePath = $@"{group.Directory.FullPath}\{file}";
                    var route = Route.GetRoute(redirectorFolder, filePath);

                    RouteFileTuples.Add(new RouteGroupTuple()
                    {
                        Route = new Route(route),
                        File = filePath
                    });
                }
            }
        }

        /// <summary>
        /// Tries to create an BF from a given route.
        /// </summary>
        /// <param name="path">The file path/route to create BF Builder for.</param>
        /// <param name="builder">The created builder.</param>
        /// <returns>True if a builder could be made, else false (if there are no files to modify this BF).</returns>
        public bool TryCreateFromPath(string path, out BfBuilder? builder)
        {
            builder = default;
            var route = new Route(Path.ChangeExtension(path, "flow"));
            foreach (var group in RouteFileTuples)
            {
                if (!route.Matches(group.Route.FullPath))
                    continue;

                // Make builder if not made.
                builder ??= new BfBuilder();

                // Add files to builder.
                builder.AddFlowFile(group.File);
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
        public string File;
    }
}
