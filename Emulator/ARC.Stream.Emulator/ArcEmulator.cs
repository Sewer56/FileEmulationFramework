using ARC.Stream.Emulator.Arc;
using ARC.Stream.Emulator.Utilities;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;

namespace ARC.Stream.Emulator;

/// <summary>
/// Simple emulator for Sega ARC files.
/// </summary>
public class ArcEmulator : IEmulator
{
    /// <summary>
    /// If enabled, dumps newly emulated files.
    /// </summary>
    public bool DumpFiles { get; set; }
    
    // Note: Handle->Stream exists because hashing IntPtr is easier; thus can resolve reads faster.
    private readonly ArcBuilderFactory _builderFactory = new();
    private Dictionary<string, MultiStream?> _pathToStream = new(StringComparer.OrdinalIgnoreCase);
    private Logger _log;

    public ArcEmulator(Logger log, bool dumpFiles)
    {
        _log = log;
        DumpFiles = dumpFiles;
    }

    public bool TryCreateFile(IntPtr handle, string filepath, string route, out IEmulatedFile emulatedFile)
    {
        // Check if we already made a custom ARC for this file.
        emulatedFile = null!;
        if (_pathToStream.TryGetValue(filepath, out var multiStream))
        {
            // Avoid recursion into same file.
            if (multiStream == null)
                return false;

            emulatedFile = new EmulatedFile<MultiStream>(multiStream);
            return true;
        }

        // Check extension.
        if (!filepath.EndsWith(Constants.ArcExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if there's a known route for this file
        // Put this before actual file check because I/O.
        if (!_builderFactory.TryCreateFromPath(filepath, out var builder))
            return false;

        // Check file type.
        if (!ArcChecker.IsArcFile(handle))
            return false;

        // Make the ARC file.
        _pathToStream[filepath] = null; // Avoid recursion into same file.

        var stream = builder!.Build(handle, filepath, _log);
        _pathToStream[filepath] = stream;
        emulatedFile = new EmulatedFile<MultiStream>(stream);

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
        _log.Info($"[{nameof(ArcEmulator)}] Dumping {filepath}");
        using var fileStream = new FileStream(filePath, FileMode.Create);
        stream.CopyTo(fileStream);
        _log.Info($"[{nameof(ArcEmulator)}] Written To {filePath}");
    }
}