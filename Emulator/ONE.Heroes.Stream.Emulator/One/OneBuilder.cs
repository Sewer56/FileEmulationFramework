using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Heroes.SDK.Definitions.Structures.Archive.OneFile;
using Heroes.SDK.Definitions.Structures.RenderWare;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Streams;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;

namespace ONE.Heroes.Stream.Emulator.One;

public class OneBuilder
{
    private List<OneBuilderItem> _builderItems = new ();
    private List<string> _filesToDelete = new ();

    /// <summary>
    /// Adds a file to the builder's input.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public void AddInputFile(string filePath)
    {
        // Get name of file.
        var fileName = Path.GetFileName(filePath.AsSpan());
        
        // Check if it's one to delete.
        if (fileName.EndsWith(Constants.DeleteExtension, StringComparison.OrdinalIgnoreCase))
        {
            AddDeleteFile(filePath[..^Constants.DeleteExtension.Length]);
            return;
        }

        if (fileName.EndsWith(Constants.CompresssedExtension, StringComparison.OrdinalIgnoreCase))
        {
            var stream = new FileSliceStreamW32(new FileSlice(filePath));
            var withoutExtension = filePath.AsSpan(0, filePath.Length - Constants.CompresssedExtension.Length);
            AddInputFile(new OneBuilderItem(stream, Path.GetFileName(withoutExtension).ToString()));
            return;
        }

        var compressed = CompressedFilesCache.GetFile(filePath);
        AddInputFile(new OneBuilderItem(compressed, Path.GetFileName(filePath)));
    }

    /// <summary>
    /// Adds a file to be deleted.
    /// </summary>
    /// <param name="fileName">Name of the file. Name only, must match what's inside ONE file, case insensitive.</param>
    public void AddDeleteFile(string fileName) => _filesToDelete.Add(fileName);

    /// <summary>
    /// Adds a file to be used as input.
    /// </summary>
    /// <param name="item">The individual item to add to the builder.</param>
    public void AddInputFile(OneBuilderItem item) => _builderItems.Add(item);

    /// <summary>
    /// Builds the ONE archive.
    /// </summary>
    public unsafe MultiStream Build(IntPtr handle, string filePath, Logger? log = null)
    {
        log?.Info($"[{nameof(OneBuilder)}] Building ONE File | {{0}}", filePath);
        
        var outputFiles = CalculateOutputFiles(handle, filePath, log, out var rwVersion).Values.ToArray();

        // Calculate sizes.
        var numberOfFiles = outputFiles.Length + 2; // Two dummy entries.
        var sizeOfHeaders = sizeof(OneArchiveHeader) + sizeof(OneNameSectionHeader);
        var sizeOfNameSection = sizeof(OneFileName) * numberOfFiles;
        var totalHeaderSize = sizeOfHeaders + sizeOfNameSection;
        
        // Make MultiStream
        var fileHeaderStream = new MemoryStream(totalHeaderSize);
        fileHeaderStream.SetLength(totalHeaderSize);
        var pairs = new List<StreamOffsetPair<Strim>>()
        {
            // Add Header
            new (fileHeaderStream, OffsetRange.FromStartAndLength(0, fileHeaderStream.Length))
        };

        // Build the damn archive.
        // Starting with the header.
        fileHeaderStream.Seek(sizeof(OneArchiveHeader), SeekOrigin.Begin); // skip header for now.
        fileHeaderStream.Write(new OneNameSectionHeader(sizeOfNameSection, rwVersion)); // name section header
        fileHeaderStream.Write(new OneFileName("")); // dummy name [Heroes is weird]
        fileHeaderStream.Write(new OneFileName("")); // dummy name [Heroes is weird]

        // Now let's deal with the files.
        int currentFileNameIndex = 2;
        int currentFilePointer = totalHeaderSize;
        int fileSectionStart = currentFilePointer;
        foreach (var outputFile in outputFiles)
        {
            // Write file name.
            fileHeaderStream.Write(new OneFileName(outputFile.Name));

            // Note: MemoryStream might be a bit heavyweight for an array with 12 bytes, but well, it is the way it is for now.
            var currentFileSize = (int)outputFile.Stream.Length;

            // Make file header.
            var header = new MemoryStream(sizeof(OneFileEntry));
            header.Write(new OneFileEntry(currentFileNameIndex++, currentFileSize, rwVersion));
            pairs.Add(new StreamOffsetPair<Strim>(header, OffsetRange.FromStartAndLength(currentFilePointer, sizeof(OneFileEntry))));

            // Make file contents.
            currentFilePointer += sizeof(OneFileEntry);
            pairs.Add(new StreamOffsetPair<Strim>(outputFile.Stream, OffsetRange.FromStartAndLength(currentFilePointer, currentFileSize)));
            currentFilePointer += currentFileSize;
        }

        // Add first header
        var fileSectionLength = currentFilePointer - fileSectionStart;
        fileHeaderStream.Seek(0, SeekOrigin.Begin);
        fileHeaderStream.Write(new OneArchiveHeader(totalHeaderSize + fileSectionLength - sizeof(OneArchiveHeader), rwVersion)); // header

        return new MultiStream(pairs);
    }

    /// <summary>
    /// Calculates the files to be used in the output ONE archive.
    /// </summary>
    private Dictionary<string, OneBuilderItem> CalculateOutputFiles(IntPtr handle, string filePath, Logger? log, out RwVersion rwVersion)
    {
        // Get Existing Files
        var outputFiles = GetExistingFiles(handle, filePath, log, out rwVersion);

        // Delete unwanted files.
        foreach (var toDelete in CollectionsMarshal.AsSpan(_filesToDelete))
            outputFiles.Remove(toDelete);

        // Add new files
        foreach (var builderItem in _builderItems)
            outputFiles[builderItem.Name] = builderItem;

        return outputFiles;
    }

    /// <summary>
    /// Retrieves the existing files from the existing ONE file.
    /// </summary>
    private unsafe Dictionary<string, OneBuilderItem> GetExistingFiles(IntPtr hFile, string filePath, Logger? logger, out RwVersion rwVersion)
    {
        var fileStream = new FileStream(new SafeFileHandle(hFile, false), FileAccess.Read);
        var originalPos = fileStream.Position;
        using var reader = new BufferedStreamReader(fileStream, 4096); // common cluster size on Windows
        var result = new Dictionary<string, OneBuilderItem>();

        // Read headers.
        reader.Read<OneArchiveHeader>(out var oneHeader);
        reader.Read<OneNameSectionHeader>(out var fileSectionHeader);
        var fileNameCount = fileSectionHeader.FileNameSectionLength / OneFileName.FileNameLength;
        rwVersion = oneHeader.RenderWareVersion;

        // Read File Names
        Span<OneFileName> fileNames = stackalloc OneFileName[fileNameCount];
        for (int x = 0; x < fileNameCount; x++)
            reader.Read(out fileNames[x]);

        // Right now we should be at address of first file.
        // Note: Heroes leaves 2 blank name slots that are unused at runtime, so file count is that and 2 less.
        for (int x = 0; x < fileNameCount - 2; x++)
        {
            var stillMoreData = reader.Position() < fileStream.Length;
            if (!stillMoreData)
                break;

            reader.Read<OneFileEntry>(out var currentEntry);
            var fileName    = fileNames[currentEntry.FileNameIndex].ToString();
            var fileDataPos = reader.Position();
            var fileSize    = currentEntry.FileSize;
#if DEBUG
            if (fileSize == 0)
            {
                logger?.Warning($"[{nameof(OneBuilder)}] Zero length file. This indicates a bug in the mod.");
                break;
            }
#endif

            var slice        = new FileSlice(fileDataPos, fileSize, filePath);
            result[fileName] = new OneBuilderItem(new FileSliceStreamW32(slice, logger), fileName);

            reader.Seek(fileSize, SeekOrigin.Current);
        }
        
        fileStream.Dispose();
        Native.SetFilePointerEx(hFile, originalPos, IntPtr.Zero, 0);
        return result;
    }
}
