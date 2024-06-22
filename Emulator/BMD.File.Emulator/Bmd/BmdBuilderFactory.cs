using AtlusScriptLibrary.Common.Libraries;
using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using System.Text;
using MessageFormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;

// Aliasing for readability, since our assembly name has priority over 'File'
using Fiel = System.IO.File;
using BMD.File.Emulator.Utilities;
using System.Text.Json.Serialization;
using System.Text.Json;
using AtlusScriptLibrary.Common.Text.Encodings;
using static BMD.File.Emulator.Utilities.CompilerArgs;

namespace BMD.File.Emulator.Bmd;

internal class BmdBuilderFactory
{
    internal List<RouteGroupTuple> RouteFileTuples = new();

    private MessageFormatVersion? _messageFormat;
    private Library? _library;
    private Encoding? _encoding;

    private Logger _log;

    public BmdBuilderFactory(Logger log)
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
                if (file.Equals(Constants.ArgsFile, StringComparison.OrdinalIgnoreCase))
                    OverrideCompilerArgs($@"{group.Directory.FullPath}\{file}");

                if (!file.EndsWith(Constants.MessageExtension, StringComparison.OrdinalIgnoreCase))
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

    public void AddFile(string fileName, string filePath, string dirPath, string route)
    {
        if (fileName.Equals(Constants.ArgsFile, StringComparison.OrdinalIgnoreCase))
            OverrideCompilerArgs(filePath);

        if (!fileName.EndsWith(Constants.MessageExtension, StringComparison.OrdinalIgnoreCase))
            return;

        RouteFileTuples.Add(new RouteGroupTuple()
        {
            Route = new Route(route),
            File = filePath
        });
    }

    /// <summary>
    /// Tries to create a BMD from a given route.
    /// </summary>
    /// <param name="path">The file path/route to create BMD Builder for.</param>
    /// <param name="builder">The created builder.</param>
    /// <returns>True if a builder could be made, else false (if there are no files to modify this BMD).</returns>
    public bool TryCreateFromPath(string path, out BmdBuilder? builder)
    {
        builder = default;

        // Add msg files
        var route = new Route(Path.ChangeExtension(path, Constants.MessageExtension));
        foreach (var group in RouteFileTuples)
        {
            if (!route.Matches(group.Route.FullPath))
                continue;

            // Make builder if not made.
            builder ??= new BmdBuilder(_messageFormat, _library, _encoding, _log);

            // Add files to builder.
            builder.AddMsgFile(group.File);
        }

        return builder != null;
    }

    private void OverrideCompilerArgs(string file)
    {
        string json = Fiel.ReadAllText(file);
        var args = JsonSerializer.Deserialize<CompilerArgs>(json);
        if (args == null)
        {
            _log.Error($"[BmdBuilderFactory] Unable to deserialise {file} to valid compiler args");
            return;
        }

        if (!Enum.TryParse(args.OutFormat, true, out OutputFileFormat outFormat))
        {
            _log.Error($"[BmdBuilderFactory] Unable parse OutFormat {args.OutFormat} to valid output format");
            return;
        }

        _messageFormat = GetMessageScriptFormatVersion(outFormat);
        _library = LibraryLookup.GetLibrary(args.Library);
        _encoding = AtlusEncoding.GetByName(args.Encoding);

        _log.Info($"[BmdBuilderFactory] Changed script compiler args to OutFormat: {args.OutFormat}, Library: {args.Library}, Encoding: {args.Encoding}");
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

