using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;
using Reloaded.Memory.Streams.Writers;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;

namespace PAK.Stream.Emulator.Pak;

public class PakBuilder
{
    private readonly Dictionary<string, FileSlice> _customFiles = new Dictionary<string, FileSlice>();
    /// <summary>
    /// Adds a file to the Virtual ARC builder.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public void AddOrReplaceFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        _customFiles[fileName] = new(filePath);
    }
    public unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = stream.Position;
        try
        {
            return BuildHelper(stream, filepath, logger);
        }
        finally
        {
            stream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    public unsafe MultiStream Build(byte[] bytes, string filepath, Logger? logger = null)
    {
        var stream = new MemoryStream(bytes);
        return BuildHelper(stream, filepath, logger);
    }
    /// <summary>
    /// Builds a PAK file.
    /// </summary>
    private unsafe MultiStream BuildHelper(Strim strim, string filepath, Logger? logger = null)
    {
        logger?.Info($"[{nameof(PakBuilder)}] Building PAK File | {{0}}", filepath);
       
        // Get original file's entries.
        IEntry[] entries = GetEntriesFromFile(strim, out var format);

        int sizeofentry;
        if (format == FormatVersion.Version1)
            sizeofentry = sizeof(V1FileEntry);
        else if (format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
            sizeofentry = sizeof(V2FileEntry);
        else
            sizeofentry = sizeof(V3FileEntry);

        Dictionary<int, FileSlice> intFiles = new Dictionary<int, FileSlice>();
        int i;
        for(i = 0; i < entries.Length; i++)
        {
            if ( _customFiles.TryGetValue(entries[i].FileName, out var customfile))
            {
                intFiles[i] = customfile;
                _customFiles.Remove(entries[i].FileName);
            }
        }
        var customArray = _customFiles.ToArray();
        for (int j = 0; j < customArray.Length; j++)
        {
            intFiles[i] = customArray[j].Value;
            i++;
        }

        var customFilesLength = intFiles.Count > 0 ? intFiles.Max(x => x.Key) + 1 : 0;
        int numFiles = Math.Max(entries.Length, customFilesLength);
        var pairs = new List<StreamOffsetPair<Strim>>();

        var currentOffset = 0;

        if (format != FormatVersion.Version1)
        {
            var headerLength = 4;
            MemoryStream headerStream = new MemoryStream(headerLength);
            var writeNum = numFiles;
            if (format == FormatVersion.Version2BE || format == FormatVersion.Version3BE)
                writeNum = Endian.Reverse(writeNum);
            headerStream.Write<int>(writeNum);
            // Make MultiStream

            currentOffset = headerLength;
            // Add Header
            pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(0, headerLength)));
        }

        var mergeAbleStreams = new List<StreamOffsetPair<Strim>>();
        var fileOffset = currentOffset;
        for (int x = 0; x < numFiles; x++)
        {
            int length = 0;
            if (intFiles.TryGetValue(x, out var overwrittenFile))
            {
                logger?.Info($"{nameof(PakBuilder)} | Injecting {{0}}, in slot {{1}}", overwrittenFile.FilePath, x);
                
                // For custom files, add to pairs directly.
                length = overwrittenFile.Length;

                
                if (format == FormatVersion.Version1)
                {
                    MemoryStream entrystream = new MemoryStream(sizeof(V1FileEntry));
                    entrystream.Write(Encoding.ASCII.GetBytes(Path.GetFileName(overwrittenFile.FilePath).PadRight(252, '\0')));
                    entrystream.Write<int>(length);
                    pairs.Add(new(entrystream, OffsetRange.FromStartAndLength(currentOffset, sizeof(V1FileEntry))));
                    
                    length = (int)Align(length, 64);
                }
                else if (format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
                {
                    MemoryStream entrystream = new MemoryStream(sizeof(V2FileEntry));
                    var writelength = format == FormatVersion.Version3BE ? Endian.Reverse(length) : length;
                    entrystream.Write(Encoding.ASCII.GetBytes(Path.GetFileName(overwrittenFile.FilePath).PadRight(24, '\0')));
                    entrystream.Write<int>(writelength);
                    pairs.Add(new(entrystream, OffsetRange.FromStartAndLength(currentOffset, sizeof(V2FileEntry))));
                }
                else
                {
                    MemoryStream entrystream = new MemoryStream(sizeof(V3FileEntry));
                    var writelength = format == FormatVersion.Version3BE ? Endian.Reverse(length) : length; 
                    entrystream.Write(Encoding.ASCII.GetBytes(Path.GetFileName(overwrittenFile.FilePath).PadRight(24, '\0')));
                    entrystream.Write<int>(writelength);
                    pairs.Add(new(entrystream, OffsetRange.FromStartAndLength(currentOffset, sizeof(V3FileEntry))));
                }


                pairs.Add(new(new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));
            }
            else if (x < entries.Length)
            {
                length = entries[x].Length;
                length = format == FormatVersion.Version2BE || format == FormatVersion.Version3BE ? Endian.Reverse(length) : length;
                length = format == FormatVersion.Version1 ? (int) Align(length, 64) : length;
                var originalEntry = new FileSlice(fileOffset, length + sizeofentry, filepath);
                var stream = new FileSliceStreamW32(originalEntry, logger);
                mergeAbleStreams.Add(new(stream, OffsetRange.FromStartAndLength(currentOffset, length + sizeofentry )));
                
            }
            else 
            {
                // Otherwise have dummy file (no-op!)
            }
            var length2 = (x < entries.Length ? entries[x].Length : 0);
            length2 = (format == FormatVersion.Version2BE || format == FormatVersion.Version3BE) ? Endian.Reverse(length2) : length2;
            length2 = format == FormatVersion.Version1 ? (int)Align(length2, 64) : length2;
            fileOffset += length2  + sizeofentry;
            // Advance offset.
            currentOffset += length + sizeofentry;
        }

        // Merge the slices and add.
        foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
            pairs.Add(merged);
        // Return MultiStream
        return new MultiStream(pairs, logger);
    }


    private static bool IsValidFormatVersion1(Strim stream)
    {
        var pos = stream.Position;
        // check if the file is too small to be a proper pak file
        if (stream.Length <= 256)
        {
            return false;
        }

        // read some test data
        if (!stream.TryRead<V1FileEntry>(out V1FileEntry fileEntry, out _))
            return false;

        stream.Position = pos;

        // check if first byte is zero, if so then no name can be stored thus making the file corrupt
        if (fileEntry.FileName == "")
            return false;


        // sanity check, if the length of the first file is >= 100 mb, fail the test
        if (fileEntry.Length >= stream.Length || fileEntry.Length < 0)
        {
            return false;
        }

        return true;
    }

    private static bool IsValidFormatVersion2And3(Strim stream, int entrySize, out bool isBigEndian)
    {
        isBigEndian = false;
        var pos = stream.Position;

        // check stream length
        if (stream.Length <= 4 + entrySize)
            return false;

        byte[] testData = new byte[4 + entrySize];
        stream.Read(testData, 0, 4 + entrySize);
        stream.Position = pos;

        int numOfFiles = BitConverter.ToInt32(testData, 0);

        // num of files sanity check
        if (numOfFiles > 1024 || numOfFiles < 1 || (numOfFiles * entrySize) > stream.Length)
        {
            numOfFiles = Endian.Reverse(numOfFiles);

            if (numOfFiles > 1024 || numOfFiles < 1 || (numOfFiles * entrySize) > stream.Length)
                return false;

            isBigEndian = true;
        }

        // check if the name field is correct
        bool nameTerminated = false;
        for (int i = 0; i < entrySize - 4; i++)
        {
            if (testData[4 + i] == 0x00)
            {
                if (i == 0)
                    return false;

                nameTerminated = true;
            }

            if (testData[4 + i] != 0x00 && nameTerminated)
                return false;
        }

        // first entry length sanity check
        int length = BitConverter.ToInt32(testData, entrySize);
        if (length >= stream.Length || length < 0)
        {
            length = Endian.Reverse(length);

            if (length >= stream.Length || length < 0)
                return false;

            isBigEndian = true;
        }

        return true;
    }
    public static FormatVersion DetectVersion(Strim stream)
    {
        if (IsValidFormatVersion1(stream))
            return FormatVersion.Version1;

        if (IsValidFormatVersion2And3(stream, 36, out var isBigEndian))
            return isBigEndian ? FormatVersion.Version2BE : FormatVersion.Version2;

        if (IsValidFormatVersion2And3(stream, 28, out isBigEndian))
            return isBigEndian ? FormatVersion.Version3BE : FormatVersion.Version3;

        return FormatVersion.Unknown;
    }
    /// <summary>
    /// Obtains the ARC header from a specific file path.
    /// </summary>
    private IEntry[] GetEntriesFromFile(Strim stream, out FormatVersion format)
    {
        format = DetectVersion(stream);
        if (format == FormatVersion.Unknown)
        {
            ThrowHelpers.IO("Unknown type of PAK file");
        }

        try
        {
            if (format != FormatVersion.Version1)
            {
                stream.TryRead(out int NumberOfFiles, out _);
                if (format == FormatVersion.Version3BE || format == FormatVersion.Version2BE)
                    NumberOfFiles = Endian.Reverse(NumberOfFiles);
                var entries = GC.AllocateUninitializedArray<IEntry>(NumberOfFiles);
                for (int i = 0; i < NumberOfFiles; i++)
                {
                    IEntry entry;
                    if (format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
                    {
                        stream.TryRead(out V2FileEntry fileentry, out _);
                        entry = fileentry;
                    }
                    else
                    {
                        stream.TryRead(out V3FileEntry fileentry, out _);
                        entry = fileentry;
                    }
                    var length = format == FormatVersion.Version3BE || format == FormatVersion.Version2BE ? Endian.Reverse(entry.Length) : entry.Length;
                    entries[i] = entry;
                    stream.Seek(length, SeekOrigin.Current);
                }
                return entries;
            }
            else
            {
                var entries = GC.AllocateUninitializedArray<IEntry>(1024);
                int i = 0;
                while (i < 1024)
                {
                    stream.TryRead<V1FileEntry>(out V1FileEntry fileentry, out _);
                    entries[i] = fileentry;
                    long roundLength = Align(fileentry.Length, 64);
                    stream.Seek(roundLength, SeekOrigin.Current);
                    i++;
                    if (stream.Length < stream.Position + 320)
                        break;
                }
                return entries.Take(i).ToArray();
            }
            
        }
        finally
        {
            stream.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Align(long value, int alignment)
    {
        return (value + (alignment - 1)) & ~(alignment - 1);
    }
}