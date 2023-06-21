using AtlusScriptLibrary.Common.Libraries;
using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using System.Text;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;

// Aliasing for readability, since our assembly name has priority over 'File'
using Fiel = System.IO.File;
using BF.File.Emulator.Utilities;
using System.Text.Json.Serialization;
using System.Text.Json;
using AtlusScriptLibrary.Common.Text.Encodings;
using static BF.File.Emulator.Utilities.CompilerArgs;

namespace BF.File.Emulator.Bf
{

    internal class BfBuilderFactory
    {

        internal List<RouteGroupTuple> RouteFileTuples = new();
        internal Dictionary<string, string> FunctionOverrides = new();
        internal Dictionary<string, string> EnumOverrides = new();

        private FlowFormatVersion? _flowFormat;
        private Library? _library;
        private Encoding? _encoding;

        private Logger _log;

        public BfBuilderFactory(Logger log)
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
                foreach (var file in group.Files)
                {
                    if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        if (file.Equals("Functions.json", StringComparison.OrdinalIgnoreCase))
                            FunctionOverrides[group.Directory.FullPath] = $@"{group.Directory.FullPath}\{file}";
                        else if (file.Equals("Enums.json", StringComparison.OrdinalIgnoreCase))
                            EnumOverrides[group.Directory.FullPath] = $@"{group.Directory.FullPath}\{file}";
                        else if (file.Equals("CompilerArgs.json", StringComparison.OrdinalIgnoreCase))
                            OverrideCompilerArgs($@"{group.Directory.FullPath}\{file}");
                    }

                    if (!file.EndsWith(".flow", StringComparison.OrdinalIgnoreCase) && !file.EndsWith(".msg", StringComparison.OrdinalIgnoreCase))
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
            // Add flow files
            var route = new Route(Path.ChangeExtension(path, "flow"));
            foreach (var group in RouteFileTuples)
            {
                if (!route.Matches(group.Route.FullPath))
                    continue;

                // Make builder if not made.
                builder ??= new BfBuilder(_flowFormat, _library, _encoding);

                // Add files to builder.
                builder.AddFlowFile(group.File);

                var dir = Path.GetDirectoryName(group.File);
                if (dir != null && FunctionOverrides.TryGetValue(dir, out var funcOverride))
                    builder.AddLibraryFile(funcOverride, _log);

                if (dir != null && EnumOverrides.TryGetValue(dir, out var enumOverride))
                    builder.AddEnumFile(enumOverride, _log);
            }

            // Add msg files for message hooks
            route = new Route(Path.ChangeExtension(path, "msg"));
            foreach (var group in RouteFileTuples)
            {
                if (!route.Matches(group.Route.FullPath))
                    continue;

                // Make builder if not made.
                builder ??= new BfBuilder(_flowFormat, _library, _encoding);

                // Add files to builder.
                builder.AddMsgFile(group.File);

                var dir = Path.GetDirectoryName(group.File);
                if (dir != null && FunctionOverrides.TryGetValue(dir, out var funcOverride))
                    builder.AddLibraryFile(funcOverride, _log);

                if (dir != null && EnumOverrides.TryGetValue(dir, out var enumOverride))
                    builder.AddEnumFile(enumOverride, _log);
            }

            return builder != null;
        }

        private void OverrideCompilerArgs(string file)
        {
            string json = Fiel.ReadAllText(file);
            var args = JsonSerializer.Deserialize<CompilerArgs>(json);
            if (args == null)
            {
                _log.Error($"[BfBuilderFactory] Unable to deserialise {file} to valid compiler args");
                return;
            }

            if(!Enum.TryParse(args.OutFormat, true, out OutputFileFormat outFormat))
            {
                _log.Error($"[BfBuilderFactory] Unable parse OutFormat {args.OutFormat} to valid output format");
                return;
            }

            _flowFormat = GetFlowScriptFormatVersion(outFormat);
            _library = LibraryLookup.GetLibrary(args.Library);
            _encoding = AtlusEncoding.GetByName(args.Encoding);

            _log.Info($"[BfBuilderFactory] Changed script compiler args to OutFormat: {args.OutFormat}, Library: {args.Library}, Encoding: {args.Encoding}");
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
