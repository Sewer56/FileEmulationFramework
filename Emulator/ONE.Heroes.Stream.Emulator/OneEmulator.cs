using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using ONE.Heroes.Stream.Emulator.One;
using ONE.Heroes.Stream.Emulator.Utilities;

namespace ONE.Heroes.Stream.Emulator;

/// <summary>
/// Simple emulator for Sonic Heroes ONE files.
/// </summary>
public class OneEmulator : IEmulator
{
    // Note: Handle->Stream exists because hashing IntPtr is easier; thus can resolve reads faster.
    private readonly OneBuilderFactory _builderFactory = new();
    private Dictionary<string, MultiStream?> _pathToStream = new(StringComparer.OrdinalIgnoreCase);
    private Logger _log;

    public OneEmulator(Logger log)
    {
        _log = log;
    }

    /// <summary>
    /// Registers a source file for archives matching a route.
    /// </summary>
    /// <param name="file">Path to the source file.</param>
    /// <param name="route">Route used to match archive paths.</param>
    internal void AddFile(string file, string route) => _builderFactory.AddFile(file, route);

    /// <summary>
    /// Registers a redirector-style ONE directory root.
    /// </summary>
    /// <param name="dir">Path to the directory root.</param>
    internal void AddDirectory(string dir) => _builderFactory.AddFromFolders(dir);

    public bool TryCreateFile(IntPtr handle, string filepath, string route, out IEmulatedFile emulatedFile)
    {
        // Check if we already made a custom ONE for this file.
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
        if (!filepath.EndsWith(Constants.OneExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if there's a known route for this file
        // Put this before actual file check because I/O.
        if (!_builderFactory.TryCreateFromPath(filepath, out var builder))
            return false;

        // Check file type.
        if (!OneChecker.IsOneFile(handle))
            return false;

        // Make the ONE file.
        _pathToStream[filepath] = null; // Avoid recursion into same file.

        var stream = builder!.Build(handle, filepath, _log);
        _pathToStream[filepath] = stream;
        emulatedFile = new EmulatedFile<MultiStream>(stream);
        
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
}
