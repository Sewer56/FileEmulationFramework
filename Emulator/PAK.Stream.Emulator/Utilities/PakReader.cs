using FileEmulationFramework.Lib.Utilities;
using PAK.Stream.Emulator.Pak;
using Reloaded.Memory;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;

namespace PAK.Stream.Emulator.Utilities;

public class PakReader
{
    public static byte[]? ReadFileFromPak(Strim fileStream, string index, string? fileRoot = null)
    {
        var pos = fileStream.Position;
        string fileName;
        string container = "";
        if (fileRoot == null)
            fileName = Path.GetFileName(index);
        else
        {
            fileName = Path.GetRelativePath(fileRoot, index).Replace("\\", "/");
            container = Path.GetDirectoryName(fileName)!.Replace("\\", "/");
        }
        fileStream.Seek(0, SeekOrigin.Begin);

        var format = PakBuilder.DetectVersion(fileStream);

        if (format == FormatVersion.Unknown)
        {
            ThrowHelpers.IO("Unknown type of PAK file");
        }

        try
        {
            switch (format)
            {
                case FormatVersion.Version1:
                    return ReadFileFromV1Pak(fileStream, fileName, container);
                case FormatVersion.Version2:
                case FormatVersion.Version2BE:
                    return ReadFileFromV2Pak(fileStream, fileName, container, format == FormatVersion.Version2BE);
                case FormatVersion.Version3:
                case FormatVersion.Version3BE:
                    return ReadFileFromV3Pak(fileStream, fileName, container, format == FormatVersion.Version3BE);
                default:
                    return null;
            }
        }
        finally
        {
            fileStream.Position = pos;
        }
    }

    private static byte[]? ReadFileFromV1Pak(Strim fileStream, string fileName, string container)
    {
        int i = 0;
        while (i < 1024)
        {
            fileStream.TryRead(out V1FileEntry entry, out _);
            if (entry.FileName == fileName)
            {
                var result = GC.AllocateUninitializedArray<byte>(entry.Length);
                fileStream.ReadAtLeast(result, entry.Length);
                return result;
            }
            else if (entry.FileName == container)
            {
                var result = GC.AllocateUninitializedArray<byte>(entry.Length);
                fileStream.ReadAtLeast(result, entry.Length);
                var file = new MemoryStream(result);
                return ReadFileFromPak(file, fileName, container);
            }

            fileStream.Seek(PakBuilder.Align(entry.Length, 64), SeekOrigin.Current);
            if (fileStream.Length < fileStream.Position + 320)
                return null;
            i++;

        }
        return null;
    }

    private static byte[]? ReadFileFromV2Pak(Strim fileStream, string fileName, string container, bool bigEndian)
    {
        fileStream.TryRead(out int numberOfFiles, out _);

        if (bigEndian)
            numberOfFiles = Endian.Reverse(numberOfFiles);

        for (int i = 0; i < numberOfFiles; i++)
        {
            fileStream.TryRead(out V2FileEntry entry, out _);
            var length = bigEndian ? Endian.Reverse(entry.Length) : entry.Length;
            if (entry.FileName == fileName)
            {
                var result = GC.AllocateUninitializedArray<byte>(length);
                fileStream.ReadAtLeast(result, length);
                return result;
            }
            else if (entry.FileName == container)
            {
                var result = GC.AllocateUninitializedArray<byte>(length);
                fileStream.ReadAtLeast(result, length);
                var file = new MemoryStream(result);
                return ReadFileFromPak(file, fileName, container);
            }

            fileStream.Seek(length, SeekOrigin.Current);
        }
        return null;
    }

    private static byte[]? ReadFileFromV3Pak(Strim fileStream, string fileName, string container, bool bigEndian)
    {
        fileStream.TryRead(out int numberOfFiles, out _);
        if (bigEndian)
            numberOfFiles = Endian.Reverse(numberOfFiles);

        for (int i = 0; i < numberOfFiles; i++)
        {
            fileStream.TryRead(out V3FileEntry entry, out _);
            var length = bigEndian ? Endian.Reverse(entry.Length) : entry.Length;
            if (entry.FileName == fileName)
            {
                var result = GC.AllocateUninitializedArray<byte>(length);
                fileStream.ReadAtLeast(result, length);
                return result;
            }
            else if (entry.FileName == container)
            {
                var result = GC.AllocateUninitializedArray<byte>(length);
                fileStream.ReadAtLeast(result, length);
                var file = new MemoryStream(result);
                return ReadFileFromPak(file, fileName, container);
            }

            fileStream.Seek(length, SeekOrigin.Current);
        }
        return null;
    }
}
