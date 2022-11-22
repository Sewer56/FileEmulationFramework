using AWB.Stream.Emulator.Awb.Structs;
using AWB.Stream.Emulator.Awb.Utilities;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Streams;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;
using ThrowHelpers = FileEmulationFramework.Lib.Utilities.ThrowHelpers;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace AWB.Stream.Emulator.Awb;

public class AwbBuilder
{
    private const int AwbAlignment = 32;
    private readonly Dictionary<int, FileSlice> _customFiles = new(); // File ID to file.
    
    /// <summary>
    /// Adds a file to the Virtual AFS builder.
    /// </summary>
    public void AddOrReplaceFile(string filePath)
    {
        var fileNameSpan = Path.GetFileNameWithoutExtension(filePath.AsSpan());

        // Trim non-numbers.
        for (int x = 0; x < fileNameSpan.Length; x++)
        {
            if (char.IsDigit(fileNameSpan[x]))
                continue;

            fileNameSpan = fileNameSpan.Slice(0, x);
            break;
        }

        if (int.TryParse(fileNameSpan, out int index))
            AddOrReplaceFile(index, filePath);
    }

    /// <summary>
    /// Adds a file to the Virtual AFS builder.
    /// </summary>
    /// <param name="index">The index associated with the file.</param>
    /// <param name="filePath">Full path to the file.</param>
    public void AddOrReplaceFile(int index, string filePath) => _customFiles[index] = new(filePath);

    public MultiStream Build(IntPtr handle, string filepath, Logger? logger = null)
    {
        // Based on AFS Redirector
        // Spec: https://github.com/tge-was-taken/010-Editor-Templates/blob/master/releases/cri_archives/cri_archives_rel_1.bt
        logger?.Info($"[{nameof(AwbBuilder)}] Building AWB File | {{0}}", filepath);
        
        // Get original file's entries.
        var entries = GetEntriesFromOriginalFile(handle, out var existingFilePos, out var subKey);
        var maxFileNo = entries.Keys.Max() + 1;
        
        // Maximum ID of AWB file.
        var customFilesLength = _customFiles.Count > 0 ? _customFiles.Max(x => x.Key) + 1 : 0;
        var numFiles = Math.Max(customFilesLength, maxFileNo);

        // Get sizes of AWB fields.
        var idFieldSize  = CalculateFieldSizeBytes(numFiles);
        var posFieldSize = CalculateFieldSizeBytes(EstimateTotalNumBytes(entries, maxFileNo, idFieldSize));
        
        // Get header size.
        var headerLength = Afs2Header.GetTotalSizeOfHeaderWithPadding(idFieldSize, numFiles, posFieldSize, AwbAlignment);
        
        // Get header stream
        var headerStream = new MemoryStream(headerLength);
        headerStream.SetLength(headerLength);

        // Write header magic and file count
        headerStream.Write<int>(Afs2Header.ExpectedMagic); // 'AFS2'
        headerStream.Write<byte>((byte)(subKey != 0 ? 2 : 1)); // 'Type'
        headerStream.Write<byte>(posFieldSize); // 'PosLength'
        headerStream.Write<byte>(idFieldSize); // 'IdLength'
        headerStream.Write<byte>(0); // 'Pad'
        headerStream.Write<int>(numFiles); // 'Entry Count'
        headerStream.Write<short>(AwbAlignment); // 'Alignment'
        headerStream.Write<short>(subKey); // 'Encryption Key'

        // Make MultiStream
        var pairs = new List<StreamOffsetPair<Strim>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, headerStream.Length))
        };
        
        // Note: This could be faster by not looping over multiple times, however as this file is only created once 
        // per run (cached) in practice, it's something I can instead write in a more readable way.
        
        // Write header IDs
        for (int x = 0; x < numFiles; x++)
        {
            if (_customFiles.TryGetValue(x, out _) || entries.ContainsKey(x))
                headerStream.WriteNumber(x, idFieldSize);
            else
                headerStream.WriteNumber(-1, idFieldSize);
        }
        
        // Write files
        var mergeAbleStreams = new List<StreamOffsetPair<Strim>>();
        var currentOffset = headerLength;
        for (int x = 0; x < numFiles; x++)
        {
            int length = 0;
            int lengthWithPadding = 0;
            if (_customFiles.TryGetValue(x, out var overwrittenFile))
            {
                logger?.Info($"{nameof(AwbBuilder)} | Injecting {{0}}, in slot {{1}}", overwrittenFile.FilePath, x);

                // For custom files, add to pairs directly.
                length = overwrittenFile.Length;
                pairs.Add(new (new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset, length)));

                // And add padding if necessary.
                lengthWithPadding = Mathematics.RoundUp(length, AwbAlignment);
                var paddingBytes = lengthWithPadding - length;
                if (paddingBytes > 0)
                    pairs.Add(new (new PaddingStream(0, paddingBytes), OffsetRange.FromStartAndLength(currentOffset + length, paddingBytes)));
            }
            else if (entries.TryGetValue(x, out var existingFile))
            {
                // Data in official CRI archives seems to use 32 byte padding. We will assume that padding is there.
                // If it is not, well, skill issue; next file will be used as padding. This is to allow merging of the streams for performance.
                length = (int)existingFile.Length;
                lengthWithPadding = Mathematics.RoundUp(length, AwbAlignment);

                var originalEntry = new FileSlice(existingFile.Position + existingFilePos, lengthWithPadding, filepath);
                var stream = new FileSliceStreamW32(originalEntry, logger);
                mergeAbleStreams.Add(new(stream, OffsetRange.FromStartAndLength(currentOffset, lengthWithPadding)));
            }
            else
            {
                // Otherwise have dummy file (no-op!)
            }

            // Write offset to header.
            headerStream.WriteNumber(length != 0 ? currentOffset : 0, posFieldSize);

            // Advance offset.
            currentOffset += lengthWithPadding;
        }

        // Write file size to end of header.
        headerStream.WriteNumber(currentOffset, posFieldSize);
        
        // Merge the slices and add.
        foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
            pairs.Add(merged);

        // Return MultiStream
        return new MultiStream(pairs, logger);
    }

    /// <summary>
    /// Estimates the total number of bytes inside resultant file assuming max size position fields.
    /// </summary>
    private long EstimateTotalNumBytes(Dictionary<int, FileEntry> entries, int numFiles, int idFieldSize)
    {
        // Get header size.
        var fileSize = (long)Afs2Header.GetTotalSizeOfHeaderWithPadding(idFieldSize, numFiles, 8, AwbAlignment);
        
        // Add size of all files.
        for (int x = 0; x < numFiles; x++)
        {
            // Sourced from custom file, else sourced from original.
            if (_customFiles.TryGetValue(x, out var overwrittenFile))
                fileSize += Mathematics.RoundUp(overwrittenFile.Length, AwbAlignment);
            else if (entries.TryGetValue(x, out var existingFile))
                fileSize += Mathematics.RoundUp(existingFile.Length, AwbAlignment);
        }

        return fileSize;
    }

    /// <summary>
    /// Calculates the field size that will be used for a given max value.
    /// </summary>
    private byte CalculateFieldSizeBytes(long maxValue)
    {
        // TODO: I don't know if CRI uses signed or unsigned, better on safe side though.
        return maxValue switch
        {
            <= short.MaxValue => sizeof(short),
            <= int.MaxValue => sizeof(int),
            <= long.MaxValue => sizeof(long)
        };
    }
    
    /// <summary>
    /// Reads in the entries from the original AWB file.
    /// </summary>
    /// <returns>Dictionary of original file ID to entry.</returns>
    private unsafe Dictionary<int, FileEntry> GetEntriesFromOriginalFile(IntPtr handle, out long existingFilePos, out short encryptionKey)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        existingFilePos = stream.Position;
        try
        {
            if (!AwbHeaderReader.TryReadHeader(stream, out var headerBytes))
                ThrowHelpers.IO("Failed to read original AWB header .");

            fixed (byte* ptr = &headerBytes![0])
            {
                var viewer = AwbViewer.FromMemory(ptr);
                var entries = GC.AllocateUninitializedArray<FileEntry>(viewer.FileCount);
                viewer.GetEntries(entries);

                var result = new Dictionary<int, FileEntry>(entries.Length);
                foreach (var entry in entries)
                    result[(int)entry.Id] = entry;

                encryptionKey = viewer.Header->EncryptionKey;
                return result;
            }
        }
        finally
        {
            stream.Dispose();
            Native.SetFilePointerEx(handle, existingFilePos, IntPtr.Zero, 0);
        }
    }
}