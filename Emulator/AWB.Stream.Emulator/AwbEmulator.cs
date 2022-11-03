using AWB.Stream.Emulator.Awb;
using AwbLib.Utilities;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;

namespace AWB.Stream.Emulator;

/// <summary>
/// Simple emulator for CRI AWB (AFS2) files.
/// </summary>
public class AwbEmulator : IEmulator
{ 
    /// <summary>
    /// If enabled, dumps newly emulated files.
    /// </summary>
    public bool DumpFiles { get; set; } = false;
    
    /// <summary>
    /// Event that is fired when a stream is created, before redirection kicks in.
    /// </summary>
    public event Action<IntPtr, MultiStream>? OnStreamCreated;
    
    // Note: Handle->Stream exists because hashing IntPtr is easier; thus can resolve reads faster.
    private readonly AwbBuilderFactory _builderFactory = new();
    private readonly Dictionary<string, MultiStream?> _pathToStream = new(StringComparer.OrdinalIgnoreCase);
    private Logger _log;

    public AwbEmulator(Logger log, bool dumpFiles)
    {
        _log = log;
        DumpFiles = dumpFiles;
    }

    public bool TryCreateFile(IntPtr handle, string filepath, string route, out IEmulatedFile emulated)
    {
        // Check if we already made a custom AWB for this file.
        emulated = null!;
        if (_pathToStream.TryGetValue(filepath, out var multiStream))
        {
            // Avoid recursion into same file.
            if (multiStream == null)
                return false;

            emulated = new EmulatedFile<MultiStream>(multiStream);
            return true;
        }

        // Check extension.
        if (!filepath.EndsWith(Constants.AwbExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if there's a known route for this file
        // Put this before actual file check because I/O.
        if (!_builderFactory.TryCreateFromPath(filepath, out var builder))
            return false;

        // Check file type.
        if (!AwbChecker.IsAwbFile(handle))
            return false;

        // Make the AFS file.
        _pathToStream[filepath] = null; // Avoid recursion into same file.

        var stream = builder!.Build(handle, filepath, _log);
        OnStreamCreated?.Invoke(handle, stream);
        _pathToStream[filepath] = stream;
        emulated = new EmulatedFile<MultiStream>(stream);

        if (DumpFiles)
            DumpFile(filepath, stream);

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
    
    private void DumpFile(string filepath, MultiStream stream)
    {
        var filePath = Path.GetFullPath($"{Constants.DumpFolder}/{Path.GetFileName(filepath)}");
        Directory.CreateDirectory(Constants.DumpFolder);
        _log.Info($"Dumping {filepath}");
        using var fileStream = new FileStream(filePath, FileMode.Create);
        stream.CopyTo(fileStream);
        _log.Info($"Written To {filePath}");
    }
}