using System.Runtime.CompilerServices;
using System.Text;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory;
using Reloaded.Memory.Streams;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace PAK.Stream.Emulator.Pak;

public class PakBuilder
{
    private readonly Dictionary<string, FileSlice> _customFiles = new Dictionary<string, FileSlice>();

    /// <summary>
    /// Adds a file to the Virtual PAK builder.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="pakName">Name of the file inside the PAK.</param>
    public void AddOrReplaceFile(string filePath, string pakName)
    {
        var nameIndex = filePath.IndexOf(pakName.Replace('\\', '/'), StringComparison.Ordinal);
        var file = filePath.Substring(nameIndex + pakName.Length + 1).Replace('\\', '/');
        _customFiles[file] = new(filePath);
    }

    /// <summary>
    /// Builds a PAK file.
    /// Adds a file to the Virtual PAK builder using its path in the pak.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="inPakPath">Path to where the file will be in the pak</param>
    public void AddOrReplaceFileWithPath(string filePath, string inPakPath)
    {
        _customFiles[inPakPath.Replace('\\', '/')] = new(filePath);
    }


    /// <summary>
    /// Builds an PAK file.
    /// </summary>
    public unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        logger?.Info($"[{nameof(PakBuilder)}] Building PAK File | {{0}}", filepath);

        // Get original file's entries.
        IEntry[] entries = GetEntriesFromFile(handle, baseOffset, out var format);

        switch (format)
        {
            case FormatVersion.Version1:
                return BuildV1(entries, handle, filepath, logger, folder, baseOffset);
            case FormatVersion.Version2:
            case FormatVersion.Version2BE:
                return BuildV2(entries, handle, filepath, format == FormatVersion.Version2BE, logger, folder, baseOffset);
            case FormatVersion.Version3:
            case FormatVersion.Version3BE:
                return BuildV3(entries, handle, filepath, format == FormatVersion.Version3BE, logger, folder, baseOffset);
            default:
                return null!; // Will nver even get here, an error is thrown in GetEntriesFromFile for invalid formats
        }
    }

    private unsafe MultiStream BuildV1(IEntry[] entries, IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        int sizeofentry = sizeof(V1FileEntry);

        Dictionary<int, FileSlice> intFiles = new Dictionary<int, FileSlice>();
        int i;

        List<string> innerPaksToEdit = new List<string>();

        int realFileNum = entries.Length;
        for (i = 0; i < entries.Length; i++)
        {
            var key = Path.Combine(folder, entries[i].FileName.Trim()).Replace('\\', '/').Replace("../", "");
            if (_customFiles.TryGetValue(key, out var customfile))
            {
                intFiles[i] = customfile;
                _customFiles.Remove(key);
            }
            if (entries[i].Length == 0 && entries[i].FileName == "")
            {
                realFileNum = i;
                break;
            }
        }
        var customArray = _customFiles.ToArray();
        for (int j = 0; j < customArray.Length; j++)
        {
            string innerPak = Path.GetDirectoryName(customArray[j].Key)!.Replace('\\', '/').Replace("../", "");
            if (innerPak == folder)
            {
                intFiles[i] = customArray[j].Value;
                _customFiles.Remove(customArray[j].Key);
                i++;
            }
            else
            {
                innerPaksToEdit.Add(innerPak);
            }
        }

        var customFilesLength = intFiles.Count > 0 ? intFiles.Max(x => x.Key) + 1 : 0;
        int numFiles = Math.Max(realFileNum, customFilesLength);
        var pairs = new List<StreamOffsetPair<Strim>>();

        long currentOffset = 0;

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

                string filename = x < entries.Length ? entries[x].FileName : Path.GetFileName(overwrittenFile.FilePath);

                MemoryStream entrystream = new MemoryStream(sizeof(V1FileEntry));
                entrystream.Write(Encoding.ASCII.GetBytes(filename.PadRight(252, '\0')));
                entrystream.Write<int>(length);
                pairs.Add(new(entrystream, OffsetRange.FromStartAndLength(currentOffset, sizeof(V1FileEntry))));

                length = (int)Align(length, 64);


                pairs.Add(new(new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset + sizeofentry, overwrittenFile.Length)));
                if (length > overwrittenFile.Length)
                {
                    var overstream = new MemoryStream();
                    byte[] buffer = new byte[length - overwrittenFile.Length];
                    for (int f = 0; i < buffer.Length; i++)
                    {
                        buffer[f] = 0x00;
                    }
                    overstream.Write(buffer, 0, buffer.Length);
                    pairs.Add(new(overstream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry + overwrittenFile.Length, buffer.Length)));
                }
            }
            else if (x < entries.Length)
            {
                length = (int)Align(entries[x].Length, 64);
                var entryContents = new FileSlice(baseOffset + fileOffset + sizeofentry, length, filepath);
                Strim entryStream = new FileSliceStreamW32(entryContents, logger);
                var container = entries[x].FileName.Trim().Replace("../", "");
                if (innerPaksToEdit.Contains(container) && DetectVersion(entryStream) != FormatVersion.Unknown)
                {
                    var entryHeader = new FileSlice(baseOffset + fileOffset, sizeofentry - 4, filepath);
                    Strim headerStream = new FileSliceStreamW32(entryHeader, logger);

                    entryStream = this.Build(handle, filepath, logger, container, baseOffset + fileOffset + sizeofentry);
                    length = (int)entryStream.Length;

                    var headerStream2 = new MemoryStream(4);
                    headerStream2.Write<int>(length);
                    pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(currentOffset, sizeofentry - 4)));
                    pairs.Add(new(headerStream2, OffsetRange.FromStartAndLength(currentOffset + sizeofentry - 4, 4)));
                    length = (int)Align(length, 64);

                    var entryStream2 = new MemoryStream(length);
                    entryStream.CopyTo(entryStream2);
                    entryStream.Dispose();
                    entryStream = entryStream2;

                    if (length > (int)entryStream.Length)
                    {
                        entryStream.Seek((int)entryStream.Length, SeekOrigin.Begin);
                        byte[] buffer = new byte[length - (int)entryStream.Length];
                        for (int f = 0; i < buffer.Length; i++)
                        {
                            buffer[f] = 0x00;
                        }
                        entryStream.Write(buffer, 0, buffer.Length);
                    }
                    pairs.Add(new(entryStream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));
                }
                else
                {
                    var entryHeader = new FileSlice(baseOffset + fileOffset, sizeofentry, filepath);
                    Strim headerStream = new FileSliceStreamW32(entryHeader, logger);

                    pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(currentOffset, sizeofentry)));
                    pairs.Add(new(entryStream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));

                }


            }
            else
            {
                // Otherwise have dummy file (no-op!)
            }
            var length2 = (int)Align(x < entries.Length ? entries[x].Length : 0, 64);
            fileOffset += length2 + sizeofentry;
            // Advance offset.
            currentOffset += length + sizeofentry;
        }

        // Merge the slices and add.
        foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
            pairs.Add(merged);
        // Return MultiStream
        return new MultiStream(pairs, logger);
    }

    private unsafe MultiStream BuildV2(IEntry[] entries, IntPtr handle, string filepath, bool bigEndian, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        int sizeofentry = sizeof(V2FileEntry);

        Dictionary<int, FileSlice> intFiles = new Dictionary<int, FileSlice>();
        int i;

        List<string> innerPaksToEdit = new List<string>();

        int realFileNum = entries.Length;
        for (i = 0; i < entries.Length; i++)
        {
            var key = Path.Combine(folder, entries[i].FileName.Trim()).Replace('\\', '/').Replace("../", "");
            if (_customFiles.TryGetValue(key, out var customfile))
            {
                intFiles[i] = customfile;
                _customFiles.Remove(key);
            }
            if (entries[i].Length == 0 && entries[i].FileName == "")
            {
                realFileNum = i;
                break;
            }
        }
        var customArray = _customFiles.ToArray();
        for (int j = 0; j < customArray.Length; j++)
        {
            string innerPak = Path.GetDirectoryName(customArray[j].Key)!.Replace('\\', '/').Replace("../", "");
            if (innerPak == folder)
            {
                intFiles[i] = customArray[j].Value;
                _customFiles.Remove(customArray[j].Key);
                i++;
            }
            else
            {
                innerPaksToEdit.Add(innerPak);
            }
        }

        var customFilesLength = intFiles.Count > 0 ? intFiles.Max(x => x.Key) + 1 : 0;
        int numFiles = Math.Max(realFileNum, customFilesLength);
        var pairs = new List<StreamOffsetPair<Strim>>();

        long currentOffset = 0;

        var headerLength = 4;
        Strim headerStream = new MemoryStream(headerLength);
        var writeNum = bigEndian ? Endian.Reverse(numFiles) : numFiles;
        headerStream.Write<int>(writeNum);

        currentOffset = headerLength;
        // Add Header
        pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(0, headerLength)));

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

                string filename = x < entries.Length ? entries[x].FileName : Path.GetFileName(overwrittenFile.FilePath);

                MemoryStream entrystream = new MemoryStream(sizeof(V2FileEntry));
                var writelength = bigEndian ? Endian.Reverse(length) : length;
                entrystream.Write(Encoding.ASCII.GetBytes(filename.PadRight(32, '\0')));
                entrystream.Write<int>(writelength);
                pairs.Add(new(entrystream, OffsetRange.FromStartAndLength(currentOffset, sizeof(V2FileEntry))));

                pairs.Add(new(new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset + sizeofentry, overwrittenFile.Length)));
                if (length > overwrittenFile.Length)
                {
                    var overstream = new MemoryStream();
                    byte[] buffer = new byte[length - overwrittenFile.Length];
                    for (int f = 0; i < buffer.Length; i++)
                    {
                        buffer[f] = 0x00;
                    }
                    overstream.Write(buffer, 0, buffer.Length);
                    pairs.Add(new(overstream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry + overwrittenFile.Length, buffer.Length)));
                }
            }
            else if (x < entries.Length)
            {
                length = entries[x].Length;
                length = bigEndian ? Endian.Reverse(length) : length;
                var entryContents = new FileSlice(baseOffset + fileOffset + sizeofentry, length, filepath);
                Strim entryStream = new FileSliceStreamW32(entryContents, logger);
                var container = entries[x].FileName.Trim().Replace("../", "");
                if (innerPaksToEdit.Contains(container) && DetectVersion(entryStream) != FormatVersion.Unknown)
                {
                    var entryHeader = new FileSlice(baseOffset + fileOffset, sizeofentry - 4, filepath);
                    headerStream = new FileSliceStreamW32(entryHeader, logger);

                    entryStream = this.Build(handle, filepath, logger, container, baseOffset + fileOffset + sizeofentry);
                    length = (int)entryStream.Length;

                    var headerStream2 = new MemoryStream(4);
                    //headerStream.Dispose();
                    headerStream2.Write<int>(bigEndian ? Endian.Reverse(length) : length);
                    pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(currentOffset, sizeofentry - 4)));
                    pairs.Add(new(headerStream2, OffsetRange.FromStartAndLength(currentOffset + sizeofentry - 4, 4)));

                    var entryStream2 = new MemoryStream(length);
                    entryStream.CopyTo(entryStream2);
                    entryStream.Dispose();
                    entryStream = entryStream2;

                    if (length > (int)entryStream.Length)
                    {
                        entryStream.Seek((int)entryStream.Length, SeekOrigin.Begin);
                        byte[] buffer = new byte[length - (int)entryStream.Length];
                        for (int f = 0; i < buffer.Length; i++)
                        {
                            buffer[f] = 0x00;
                        }
                        entryStream.Write(buffer, 0, buffer.Length);
                    }
                    pairs.Add(new(entryStream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));
                }
                else
                {
                    var entryHeader = new FileSlice(baseOffset + fileOffset, sizeofentry, filepath);
                    headerStream = new FileSliceStreamW32(entryHeader, logger);

                    pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(currentOffset, sizeofentry)));
                    pairs.Add(new(entryStream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));

                }


            }
            else
            {
                // Otherwise have dummy file (no-op!)
            }
            var length2 = (x < entries.Length ? entries[x].Length : 0);
            length2 = bigEndian ? Endian.Reverse(length2) : length2;
            fileOffset += length2 + sizeofentry;
            // Advance offset.
            currentOffset += length + sizeofentry;
        }

        // Merge the slices and add.
        foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
            pairs.Add(merged);
        // Return MultiStream
        return new MultiStream(pairs, logger);
    }

    private unsafe MultiStream BuildV3(IEntry[] entries, IntPtr handle, string filepath, bool bigEndian, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        int sizeofentry = sizeof(V3FileEntry);

        Dictionary<int, FileSlice> intFiles = new Dictionary<int, FileSlice>();
        int i;

        List<string> innerPaksToEdit = new List<string>();

        int realFileNum = entries.Length;
        for (i = 0; i < entries.Length; i++)
        {
            var key = Path.Combine(folder, entries[i].FileName.Trim()).Replace('\\', '/').Replace("../", "");
            if (_customFiles.TryGetValue(key, out var customfile))
            {
                intFiles[i] = customfile;
                _customFiles.Remove(key);
            }
            if (entries[i].Length == 0 && entries[i].FileName == "")
            {
                realFileNum = i;
                break;
            }
        }
        var customArray = _customFiles.ToArray();
        for (int j = 0; j < customArray.Length; j++)
        {
            string innerPak = Path.GetDirectoryName(customArray[j].Key)!.Replace('\\', '/').Replace("../", "");
            if (innerPak == folder)
            {
                intFiles[i] = customArray[j].Value;
                _customFiles.Remove(customArray[j].Key);
                i++;
            }
            else
            {
                innerPaksToEdit.Add(innerPak);
            }
        }

        var customFilesLength = intFiles.Count > 0 ? intFiles.Max(x => x.Key) + 1 : 0;
        int numFiles = Math.Max(realFileNum, customFilesLength);
        var pairs = new List<StreamOffsetPair<Strim>>();

        long currentOffset = 0;

        var headerLength = 4;
        Strim headerStream = new MemoryStream(headerLength);
        var writeNum = bigEndian ? Endian.Reverse(numFiles) : numFiles;
        headerStream.Write<int>(writeNum);

        currentOffset = headerLength;
        // Add Header
        pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(0, headerLength)));

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

                string filename = x < entries.Length ? entries[x].FileName : Path.GetFileName(overwrittenFile.FilePath);

                MemoryStream entrystream = new MemoryStream(sizeof(V3FileEntry));
                var writelength = bigEndian ? Endian.Reverse(length) : length;
                entrystream.Write(Encoding.ASCII.GetBytes(filename.PadRight(24, '\0')));
                entrystream.Write<int>(writelength);
                pairs.Add(new(entrystream, OffsetRange.FromStartAndLength(currentOffset, sizeof(V3FileEntry))));

                pairs.Add(new(new FileSliceStreamW32(overwrittenFile, logger), OffsetRange.FromStartAndLength(currentOffset + sizeofentry, overwrittenFile.Length)));
                if (length > overwrittenFile.Length)
                {
                    var overstream = new MemoryStream();
                    byte[] buffer = new byte[length - overwrittenFile.Length];
                    for (int f = 0; i < buffer.Length; i++)
                    {
                        buffer[f] = 0x00;
                    }
                    overstream.Write(buffer, 0, buffer.Length);
                    pairs.Add(new(overstream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry + overwrittenFile.Length, buffer.Length)));
                }
            }
            else if (x < entries.Length)
            {
                length = entries[x].Length;
                length = bigEndian ? Endian.Reverse(length) : length;
                var entryContents = new FileSlice(baseOffset + fileOffset + sizeofentry, length, filepath);
                Strim entryStream = new FileSliceStreamW32(entryContents, logger);
                var container = entries[x].FileName.Trim().Replace("../", "");
                if (innerPaksToEdit.Contains(container) && DetectVersion(entryStream) != FormatVersion.Unknown)
                {
                    var entryHeader = new FileSlice(baseOffset + fileOffset, sizeofentry - 4, filepath);
                    headerStream = new FileSliceStreamW32(entryHeader, logger);

                    entryStream = this.Build(handle, filepath, logger, container, baseOffset + fileOffset + sizeofentry);
                    length = (int)entryStream.Length;

                    var headerStream2 = new MemoryStream(4);
                    headerStream2.Write<int>(bigEndian ? Endian.Reverse(length) : length);
                    pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(currentOffset, sizeofentry - 4)));
                    pairs.Add(new(headerStream2, OffsetRange.FromStartAndLength(currentOffset + sizeofentry - 4, 4)));

                    var entryStream2 = new MemoryStream(length);
                    entryStream.CopyTo(entryStream2);
                    entryStream.Dispose();
                    entryStream = entryStream2;

                    if (length > (int)entryStream.Length)
                    {
                        entryStream.Seek((int)entryStream.Length, SeekOrigin.Begin);
                        byte[] buffer = new byte[length - (int)entryStream.Length];
                        for (int f = 0; i < buffer.Length; i++)
                        {
                            buffer[f] = 0x00;
                        }
                        entryStream.Write(buffer, 0, buffer.Length);
                    }
                    pairs.Add(new(entryStream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));
                }
                else
                {
                    var entryHeader = new FileSlice(baseOffset + fileOffset, sizeofentry, filepath);
                    headerStream = new FileSliceStreamW32(entryHeader, logger);

                    pairs.Add(new(headerStream, OffsetRange.FromStartAndLength(currentOffset, sizeofentry)));
                    pairs.Add(new(entryStream, OffsetRange.FromStartAndLength(currentOffset + sizeofentry, length)));
                }
            }
            else
            {
                // Otherwise have dummy file (no-op!)
            }
            var length2 = (x < entries.Length ? entries[x].Length : 0);
            length2 = bigEndian ? Endian.Reverse(length2) : length2;
            fileOffset += length2 + sizeofentry;
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
        try
        {
            // check if the file is too small to be a proper pak file
            if (stream.Length <= 256)
            {
                return false;
            }

            // read some test data
            byte[] testData = new byte[256];
            stream.ReadAtLeast(testData, 256);

            // check if first byte is zero, if so then no name can be stored thus making the file corrupt
            if (testData[0] == 0x00)
                return false;

            bool nameTerminated = false;
            for (int i = 0; i < 252; i++)
            {
                if (testData[i] == 0x00)
                    nameTerminated = true;

                // If the name has already been terminated but there's still data in the reserved space,
                // fail the test
                if (nameTerminated && testData[i] != 0x00)
                    return false;
            }

            int testLength = BitConverter.ToInt32(testData, 252);

            // sanity check, if the length of the first file is >= 100 mb, fail the test
            if (testLength >= stream.Length || testLength < 0)
            {
                return false;
            }

            return true;

        }
        finally
        {
            stream.Seek(pos, SeekOrigin.Begin);
        }
    }

    private static bool IsValidFormatVersion2And3(Strim stream, int entrySize, out bool isBigEndian)
    {
        isBigEndian = false;
        var pos = stream.Position;
        try
        {
            // check stream length
            if (stream.Length <= 4 + entrySize)
                return false;

            byte[] testData = GC.AllocateUninitializedArray<byte>(4 + entrySize);
            stream.ReadAtLeast(testData, 4 + entrySize);
            int numOfFiles = BitConverter.ToInt32(testData, 0);

            // num of files sanity check
            if (numOfFiles is > 1024 or < 0 || (numOfFiles * entrySize) > stream.Length)
            {
                numOfFiles = Endian.Reverse(numOfFiles);

                if (numOfFiles is > 1024 or < 1 || (numOfFiles * entrySize) > stream.Length)
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
        finally
        {
            stream.Seek(pos, SeekOrigin.Begin);
        }
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
    /// Obtains the PAK header from a specific file path.
    /// </summary>
    private IEntry[] GetEntriesFromFile(IntPtr handle, long pos, out FormatVersion format)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        stream.Seek(pos, SeekOrigin.Begin);

        format = DetectVersion(stream);
        if (format == FormatVersion.Unknown)
        {
            ThrowHelpers.IO("Unknown type of PAK file");
        }

        try
        {
            if (format != FormatVersion.Version1)
            {
                stream.TryRead(out int numberOfFiles, out _);
                if (format is FormatVersion.Version3BE or FormatVersion.Version2BE)
                    numberOfFiles = Endian.Reverse(numberOfFiles);

                var entries = GC.AllocateUninitializedArray<IEntry>(numberOfFiles);
                for (int i = 0; i < numberOfFiles; i++)
                {
                    IEntry entry;
                    if (format == FormatVersion.Version2 || format == FormatVersion.Version2BE)
                    {
                        stream.TryRead(out V2FileEntry fileEntry, out _);
                        entry = fileEntry;
                    }
                    else
                    {
                        stream.TryRead(out V3FileEntry fileEntry, out _);
                        entry = fileEntry;
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
                    stream.TryRead<V1FileEntry>(out V1FileEntry fileEntry, out _);
                    entries[i] = fileEntry;
                    long roundLength = Align(fileEntry.Length, 64);
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
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Align(long value, int alignment)
    {
        return (value + (alignment - 1)) & ~(alignment - 1);
    }
}