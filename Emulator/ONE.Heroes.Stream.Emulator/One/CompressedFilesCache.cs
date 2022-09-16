using csharp_prs_interfaces;
using FileEmulationFramework.Lib.Memory;

namespace ONE.Heroes.Stream.Emulator.One;

/// <summary>
/// Utility class that compresses incoming files and stores them in the pagefile as needed.
/// </summary>
public static class CompressedFilesCache
{ 
    private static Dictionary<string, MemoryManagerStream> _pathToStream = new (StringComparer.OrdinalIgnoreCase);
    private static IPrsInstance _prs = null!;
    private static int _searchWindowSize;

    /// <summary>
    /// Initializes the compressed cache.
    /// </summary>
    /// <param name="prs">Instance of the PRS compressor.</param>
    /// <param name="searchWindowSize">Search of search window for compressed data.</param>
    public static void Init(IPrsInstance prs, int searchWindowSize = 255)
    {
        _prs = prs;
        _searchWindowSize = searchWindowSize;
    }

    /// <summary>
    /// Gets file from the pagefile backed cache; if not present, compresses it and adds to cache.
    /// </summary>
    /// <param name="filePath">Full path to the uncompressed file.</param>
    public static MemoryManagerStream GetFile(string filePath)
    {
        if (TryGetExistingFile(filePath, out var stream))
            return stream!;

        // Compress the file.
        var data = File.ReadAllBytes(filePath);
        var compressed = _prs.Compress(data, _searchWindowSize);

        // Assign the stream.
        var manager = new MemoryManager(65536);
        stream = new MemoryManagerStream(manager, true);
        stream.Write(compressed);
        _pathToStream[filePath] = stream;

        return stream;
    }

    /// <summary>
    /// Tries to get an existing file if possible.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="stream">Existing file.</param>
    /// <returns>An existing file, if present, else nothing.</returns>
    public static bool TryGetExistingFile(string filePath, out MemoryManagerStream? stream) => _pathToStream.TryGetValue(filePath, out stream);

    /// <summary>
    /// Disposes of all.
    /// For testing use only.
    /// </summary>
    public static void Clear()
    {
        foreach (var pathToStreams in _pathToStream) 
            pathToStreams.Value.Dispose();

        _pathToStream.Clear();
    }
}