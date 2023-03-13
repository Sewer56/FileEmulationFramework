using System.Runtime.InteropServices;
using AFSLib.AfsStructs;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Streams;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace AFS.Stream.Emulator.Afs;

public class AfsBuilder
{
    private const int AfsAlignment = 2048;
    private readonly Dictionary<int, FileSlice> _customFiles = new();

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
    public void AddOrReplaceFile(int index, string filePath)
    {
        if (index > ushort.MaxValue)
            ThrowHelpers.Argument($"[{nameof(AfsBuilder)}] Attempted to add file with index > {index}, this is not supported by the AFS container.");

        _customFiles[index] = new(filePath);
    }

    /// <summary>
    /// Builds an AFS file.
    /// </summary>
    public unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null)
    {
        // Spec: http://wiki.xentax.com/index.php/GRAF:AFS_AFS
        logger?.Info($"[{nameof(AfsBuilder)}] Building AFS File | {{0}}", filepath);

        // Get original file's entries.
        var entries = GetEntriesFromFile(handle);
        
        // Maximum ID of AFS file.
        var customFilesLength = _customFiles.Count > 0 ? _customFiles.Max(x => x.Key) + 1 : 0;
        var numFiles    = Math.Max(customFilesLength, entries.Length);

        // Allocate Header
        // Note: We do not emit optional pointer to file metadata, but must reserve space for its null pointer, hence +1
        var headerLength = sizeof(AfsHeader) + (sizeof(AfsFileEntry) * (numFiles + 1));
        headerLength = Mathematics.RoundUp(headerLength, AfsAlignment);
        var headerStream = new MemoryStream(headerLength);
        headerStream.SetLength(headerLength);

        // Write header magic and file count
        headerStream.Write<int>(0x00534641); // 'AFS '
        headerStream.Write<int>(numFiles);

        // Make MultiStream
        var pairs = new List<StreamOffsetPair<Strim>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, headerStream.Length))
        };

        var mergeAbleStreams = new List<StreamOffsetPair<Strim>>();
        var currentOffset = headerLength;
        for (int x = 0; x < numFiles; x++)
        {
            int length = 0;
            int lengthWithPadding = 0;
            if (_customFiles.TryGetValue(x, out var overwrittenFile))
            {
                logger?.Info($"{nameof(AfsBuilder)} | Injecting {{0}}, in slot {{1}}", overwrittenFile.FilePath, x);

                // For custom files, add to pairs directly.
                length = overwrittenFile.Length;
                pairs.Add(new (new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset, length)));

                // And add padding if necessary.
                lengthWithPadding = Mathematics.RoundUp(length, AfsAlignment);
                var paddingBytes = lengthWithPadding - length;
                if (paddingBytes > 0)
                    pairs.Add(new (new PaddingStream(0, paddingBytes), OffsetRange.FromStartAndLength(currentOffset + length, paddingBytes)));
            }
            else if (x < entries.Length)
            {
                // Data in official CRI archives use 2048 byte padding. We will assume that padding is there.
                // If it is not, well, skill issue; next file will be used as padding. This is to allow merging of the streams for performance.
                length = entries[x].Length;
                lengthWithPadding = Mathematics.RoundUp(length, AfsAlignment);

                var originalEntry = new FileSlice(entries[x].Offset, lengthWithPadding, filepath);
                var stream = new FileSliceStreamW32(originalEntry, logger);
                mergeAbleStreams.Add(new(stream, OffsetRange.FromStartAndLength(currentOffset, lengthWithPadding)));
            }
            else
            {
                // Otherwise have dummy file (no-op!)
            }

            // Write offset + length to header.
            headerStream.Write(length != 0 ? currentOffset : 0);
            headerStream.Write(length);

            // Advance offset.
            currentOffset += lengthWithPadding;
        }

        // Merge the slices and add.
        foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
            pairs.Add(merged);

        // Return MultiStream
        return new MultiStream(pairs, logger);
    }

    /// <summary>
    /// Obtains the AFS header from a specific file path.
    /// </summary>
    private AfsFileEntry[] GetEntriesFromFile(IntPtr handle)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = stream.Position;
        try
        {
            if (!stream.TryRead(out AfsHeader header, out _))
                ThrowHelpers.IO("Failed to read original AFS header start.");

            var entries = GC.AllocateUninitializedArray<AfsFileEntry>(header.NumberOfFiles);
            if (!stream.TryRead(MemoryMarshal.Cast<AfsFileEntry, byte>(entries), out _))
                ThrowHelpers.IO("Failed to read original AFS header pos+offset.");

            return entries;
        }
        finally
        {
            stream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}