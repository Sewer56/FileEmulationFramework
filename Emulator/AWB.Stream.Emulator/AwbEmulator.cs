using AWB.Stream.Emulator.Awb;
using AWB.Stream.Emulator.Awb.Utilities;
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
    public bool DumpFiles { get; set; }
    
    /// <summary>
    /// Event that is fired when a stream is created, before redirection kicks in.
    /// </summary>
    public event Action<IntPtr, string, MultiStream>? OnStreamCreated;
    
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

        if (!TryCreateEmulatedFile(handle, filepath, filepath, filepath, true, ref emulated!, out _)) 
            return false;

        return true;
    }

    /// <summary>
    /// Tries to create an emulated file from a given file handle.
    /// </summary>
    /// <param name="handle">Handle of the file where the data is sourced from.</param>
    /// <param name="srcDataPath">Path of the file where the handle refers to.</param>
    /// <param name="outputPath">Path where the emulated file is stored.</param>
    /// <param name="route">The route of the emulated file, for builder to pick up.</param>
    /// <param name="invokeOnStreamCreated">Invokes the <see cref="OnStreamCreated"/> event.</param>
    /// <param name="emulated">The emulated file.</param>
    /// <param name="stream">The created stream under the hood.</param>
    /// <returns></returns>
    public bool TryCreateEmulatedFile(IntPtr handle, string srcDataPath, string outputPath, string route, bool invokeOnStreamCreated, ref IEmulatedFile? emulated, out MultiStream? stream)
    {
        stream = null;
        
        // Check if there's a known route for this file
        // Put this before actual file check because I/O.
        if (!_builderFactory.TryCreateFromPath(route, out var builder))
            return false;

        // Check file type.
        if (!AwbChecker.IsAwbFile(handle))
            return false;

        // Make the AFS file.
        _pathToStream[outputPath] = null; // Avoid recursion into same file.

        stream = builder!.Build(handle, srcDataPath, _log);
        if (invokeOnStreamCreated)
            OnStreamCreated?.Invoke(handle, outputPath, stream);
        
        _pathToStream[outputPath] = stream;
        emulated = new EmulatedFile<MultiStream>(stream);
        _log.Info("[AwbEmulator] Created Emulated file with Path {0}", outputPath);

        if (DumpFiles)
            Utility.DumpFile(_log, outputPath, stream);
        
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
    /// Invalidates an AWB file with a specified name.
    /// </summary>
    /// <param name="awbPath">Full path to the file.</param>
    public void UnregisterFile(string awbPath) => _pathToStream.Remove(awbPath);

    internal List<RouteGroupTuple> GetInput() => _builderFactory.RouteGroupTuples;
    
    /// <summary>
    /// Manually invokes the <see cref="OnStreamCreated"/> event. For internal use only.
    /// </summary>
    internal void InvokeOnStreamCreated(IntPtr handle, string outputPath, MultiStream stream) => OnStreamCreated?.Invoke(handle, outputPath, stream);
}