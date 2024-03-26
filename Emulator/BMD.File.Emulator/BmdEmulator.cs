using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using BMD.File.Emulator.Bmd;
using FileEmulationFramework.Lib.Memory;
using BMD.File.Emulator.Utilities;
using MessageFormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;
using static BMD.File.Emulator.Mod;
using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Text.Encodings;
using AtlusScriptLibrary.Common.Logging;
using Logger = FileEmulationFramework.Lib.Utilities.Logger;
using System.Diagnostics;

namespace BMD.File.Emulator;

/// <summary>
/// Emulator for Atlus BMD files.
/// </summary>
public class BmdEmulator : IEmulator
{
    /// <summary>
    /// If enabled, dumps newly emulated files.
    /// </summary>
    public bool DumpFiles { get; set; }

    // Note: Handle->Stream exists because hashing IntPtr is easier; thus can resolve reads faster.
    private readonly BmdBuilderFactory _builderFactory;
    private readonly ConcurrentDictionary<string, Stream?> _pathToStream = new(StringComparer.OrdinalIgnoreCase);
    private Logger _log;

    private MessageFormatVersion _messageFormat;
    private Library _library;
    private Encoding _encoding;
    private AtlusLogListener _listener;

    public BmdEmulator(Logger log, bool dumpFiles, Game game)
    {
        _log = log;
        _builderFactory = new BmdBuilderFactory(log);
        DumpFiles = dumpFiles;
        _listener = new AtlusLogListener(log, LogLevel.Error);

        switch (game)
        {
            case Game.P3P:
                _messageFormat = MessageFormatVersion.Version1;
                _library = LibraryLookup.GetLibrary("P3P");
                _encoding = AtlusEncoding.GetByName("P4");
                break;
            case Game.P4G:
                _messageFormat = MessageFormatVersion.Version1;
                _library = LibraryLookup.GetLibrary("P4G");
                _encoding = AtlusEncoding.GetByName("P4");
                break;
            case Game.P5R:
                _messageFormat = MessageFormatVersion.Version1BigEndian;
                _library = LibraryLookup.GetLibrary("P5R");
                _encoding = AtlusEncoding.GetByName("P5R");
                break;
        }
    }

    public bool TryCreateFile(IntPtr handle, string filepath, string route, out IEmulatedFile emulated)
    {
        // Check if we already made a custom BMD for this file.
        emulated = null!;
        if (_pathToStream.TryGetValue(filepath, out var stream))
        {
            // Avoid recursion into same file.
            if (stream == null)
                return false;

            emulated = new EmulatedFile<Stream>(stream);
            return true;
        }

        // Check extension.
        if (!filepath.EndsWith(Constants.BmdExtension, StringComparison.OrdinalIgnoreCase) || filepath.EndsWith(Constants.DumpExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!TryCreateEmulatedFile(handle, filepath, filepath, filepath, ref emulated!, out _))
            return false;

        return true;
    }

    /// <summary>
    /// Tries to create an emulated file from a given file handle.
    /// </summary>
    /// <param name="handle">Handle of the bmd file to use as a base.</param>
    /// <param name="srcDataPath">Path of the file the handle refers to.</param>
    /// <param name="outputPath">Path where the emulated file is stored.</param>
    /// <param name="route">The route of the emulated file, for builder to pick up.</param>
    /// <param name="emulated">The emulated file.</param>
    /// <param name="stream">The created stream under the hood.</param>
    /// <returns>True if an emulated file could be created, false otherwise</returns>
    public bool TryCreateEmulatedFile(IntPtr handle, string srcDataPath, string outputPath, string route, ref IEmulatedFile? emulated, out Stream? stream)
    {
        stream = null;

        // Check if there's a known route for this file
        // Put this before actual file check because I/O.
        if (!_builderFactory.TryCreateFromPath(route, out var builder))
            return false;

        // Check file type.
        if (!BmdChecker.IsBmdFileOrEmpty(handle, out var isEmpty))
            return false;

        // Make the BMD file.
        _pathToStream[outputPath] = null; // Avoid recursion into same file.

        stream = builder!.Build(handle, srcDataPath, _messageFormat, _library, _encoding, _listener, isEmpty);
        if (stream == null)
            return false;

        _pathToStream.TryAdd(outputPath, stream);
        emulated = new EmulatedFile<Stream>(stream);
        _log.Info("[BmdEmulator] Created Emulated file with Path {0}", outputPath);

        if (DumpFiles)
            DumpFile(route, stream);

        return true;
    }

    /// <summary>
    /// Called when a mod is being loaded.
    /// </summary>
    /// <param name="modFolder">Folder where the mod is contained.</param>
    public void OnModLoading(string modFolder)
    {
        var redirectorFolder = $"{modFolder}/{Constants.RedirectorFolder}";

        if (Directory.Exists(redirectorFolder))
            _builderFactory.AddFromFolders(redirectorFolder);
    }

    /// <summary>
    /// Invalidates a BMD file with a specified name.
    /// </summary>
    /// <param name="bmdPath">Full path to the file.</param>
    public void UnregisterFile(string bmdPath)
    {
        _pathToStream!.Remove(bmdPath, out var stream);
        stream?.Dispose();
    }

    public void RegisterFile(string destinationPath, Stream stream)
    {
        _pathToStream.TryAdd(destinationPath, stream);
    }

    private void DumpFile(string route, Stream stream)
    {
        var dumpPath = Path.GetFullPath($"{Constants.DumpFolder}/{Path.ChangeExtension(route, Constants.DumpExtension)}");
        Directory.CreateDirectory(Path.GetDirectoryName(dumpPath));
        _log.Info($"[BmdEmulator] Dumping {route}");
        using var fileStream = new FileStream(dumpPath, FileMode.Create);
        stream.CopyTo(fileStream);
        _log.Info($"[BmdEmulator] Written To {dumpPath}");
    }

    internal List<RouteGroupTuple> GetInput() => _builderFactory.RouteFileTuples;

    internal void AddFromFolders(string dir) => _builderFactory.AddFromFolders(dir);

    internal void AddFile(string file, string route) => _builderFactory.AddFile(Path.GetFileName(file), file, Path.GetDirectoryName(file)!, route);
}

public class AtlusLogListener : LogListener
{

    private Logger _logger;

    internal AtlusLogListener(Logger logger, LogLevel logLevel) : base(logLevel)
    {
        _logger = logger;
    }

    protected override void OnLogCore(object sender, LogEventArgs e)
    {
        switch (e.Level)
        {
            case LogLevel.Info:
                _logger.Info("[Script Compiler] {0}", e.Message);
                break;
            case LogLevel.Warning:
                _logger.Warning("[Script Compiler] {0}", e.Message);
                break;
            case LogLevel.Error:
                _logger.Error("[Script Compiler] {0}", e.Message);
                break;
            case LogLevel.Debug:
                _logger.Debug("[Script Compiler] {0}", e.Message);
                break;
            case LogLevel.Fatal:
                _logger.Fatal("[Script Compiler] {0}", e.Message);
                break;
            default:
                _logger.Info("[Script Compiler] {0}", e.Message);
                break;
        };
    }
}
