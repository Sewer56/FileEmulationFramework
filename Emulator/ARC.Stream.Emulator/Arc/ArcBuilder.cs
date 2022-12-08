using System.Runtime.InteropServices;
using System.Text;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Streams;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;

namespace ARC.Stream.Emulator.Arc;

public class ArcBuilder
{
    private readonly Dictionary<string, FileSlice> _customFiles = new Dictionary<string, FileSlice>();
    /// <summary>
    /// Adds a file to the Virtual ARC builder.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public void AddOrReplaceFile(string filePath)
    {
        string[] filePathSplit = filePath.Split(".ARC" + Path.DirectorySeparatorChar);
        _customFiles[filePathSplit[^1]] = new FileSlice(filePath);

    }

    /// <summary>
    /// Builds an ARC file.
    /// </summary>
    public unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null)
    {
        logger?.Info($"[{nameof(ArcBuilder)}] Building ARC File | {{0}}", filepath);

        // Get original file's entries.
        ArcFileEntry[] entries = GetEntriesFromFile(handle);
        ushort numFiles = (ushort)entries.Length;

        // Allocate Header
        int headerLength = sizeof(ArcHeader) + sizeof(ArcFileEntry) * numFiles;
        MemoryStream headerStream = new MemoryStream(headerLength);

        // Write header magic and file count
        headerStream.Write( 0x4C435241 ); // 'ARCL'
        headerStream.Write(numFiles);

        // Make MultiStream
        var pairs = new List<StreamOffsetPair<Strim>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, headerLength))
        };

        RandXoringStream headerStream2 = new RandXoringStream(headerStream, numFiles);

        var mergeAbleStreams = new List<StreamOffsetPair<Strim>>();
        var currentOffset = headerLength;
        for (int x = 0; x < numFiles; x++)
        {
            int length = 0;
            string filename = entries[x].GetFileName();
            if (_customFiles.TryGetValue(filename, out var overwrittenFile))
            {
                logger?.Info($"{nameof(ArcBuilder)} | Injecting {{0}}, in slot {{1}}", overwrittenFile.FilePath, x);

                // For custom files, add to pairs directly.
                length = overwrittenFile.Length;
                pairs.Add(new (new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset, length)));
            }
            else if (x < entries.Length)
            {
                
                length = (int)entries[x].Length;
               
                var originalEntry = new FileSlice(entries[x].Offset, length, filepath);
                var stream = new FileSliceStreamW32(originalEntry, logger);
                mergeAbleStreams.Add(new(stream, OffsetRange.FromStartAndLength(currentOffset, length)));
            }
            else
            {
                // Otherwise have dummy file (no-op!)
            }

            // Write filename + offset + length to header.
            headerStream2.Write(Encoding.ASCII.GetBytes(filename.PadRight(64, '\0')));
            headerStream2.Write(currentOffset);
            headerStream2.Write(length);

            // Advance offset.
            currentOffset += length;
        }

        // Merge the slices and add.
        foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
            pairs.Add(merged);
        // Return MultiStream
        return new MultiStream(pairs, logger);
    }

    /// <summary>
    /// Obtains the ARC header from a specific file path.
    /// </summary>
    private ArcFileEntry[] GetEntriesFromFile(IntPtr handle)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = stream.Position;
        try
        {
            if (!stream.TryRead(out ArcHeader header, out _))
                ThrowHelpers.IO("Failed to read original ARC header start.");

            var entries = GC.AllocateUninitializedArray<ArcFileEntry>(header.GetNumberOfFiles());
            RandXoringStream input = new RandXoringStream(stream, header.GetNumberOfFiles());
            if (!input.TryRead(MemoryMarshal.Cast<ArcFileEntry, byte>(entries), out _))
                ThrowHelpers.IO("Failed to read original ARC header pos+offset.");

            input.Dispose();
            return entries;
        }
        finally
        {
            stream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }
}