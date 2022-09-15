using AFS.Stream.Emulator.Afs;
using AFS.Stream.Emulator.Utilities;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;

namespace AFS.Stream.Emulator;

/// <summary>
/// Simple emulator for CRI AFS files.
/// </summary>
public class AfsEmulator : IEmulator
{
    // Note: Handle->Stream exists because hashing IntPtr is easier; thus can resolve reads faster.
    private readonly AfsBuilderFactory _builderFactory = new();
    private Dictionary<IntPtr, MultiStream> _handleToStream = new();
    private Dictionary<string, MultiStream?> _pathToStream = new(StringComparer.OrdinalIgnoreCase);
    private Logger _log;

    public AfsEmulator(Logger log) => _log = log;

    public string Folder => Constants.RedirectorFolder;

    public bool TryCreateFile(IntPtr handle, string filepath, string route)
    {
        // Check if we already made a custom AFS for this file.
        if (_pathToStream.TryGetValue(filepath, out var multiStream))
        {
            // Avoid recursion into same file.
            if (multiStream == null)
                return false;

            _handleToStream[handle] = multiStream;
            return true;
        }

        // Check extension.
        if (!filepath.EndsWith(Constants.AfsExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if there's a known route for this file
        // Put this before actual file check because I/O.
        if (!_builderFactory.TryCreateFromPath(filepath, out var builder))
            return false;

        // Check file type.
        if (!AfsChecker.IsAfsFile(handle))
            return false;

        // Make the AFS file.
        _pathToStream[filepath] = null; // Avoid recursion into same file.

        var stream = builder!.Build(handle, filepath, _log);
        _pathToStream[filepath] = stream;
        _handleToStream[handle] = stream;
        
        return true;
    }

    public long GetFileSize(IntPtr handle, IFileInformation info) => _handleToStream[handle].Length;

    public unsafe bool ReadData(IntPtr handle, byte* buffer, uint length, long offset, IFileInformation info, out int numReadBytes)
    {
        var stream     = _handleToStream[handle];
        var bufferSpan = new Span<byte>(buffer, (int)length);
        stream.Seek(offset, SeekOrigin.Begin);
        stream.TryRead(bufferSpan, out numReadBytes);
        return numReadBytes > 0;
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