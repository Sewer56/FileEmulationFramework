using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Interfaces;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory;
using Reloaded.Memory.Extensions;
using Reloaded.Memory.Streams;

namespace SPD.File.Emulator.Spd;

public class SpdBuilder
{
    private Dictionary<string, FileSlice> _customSprFiles = new();
    private Dictionary<string, FileSlice> _customDdsFiles = new();
    private Dictionary<int, MemoryStream> _textureData = new();
    private SpdTextureDictionary _textureEntries = new();
    private SpdSpriteDictionary _spriteEntries = new();

    private SpdHeader _spdHeader;

    Logger _log;

    public SpdBuilder(Logger log)
    {
        _log = log;
    }

    /// <summary>
    /// Adds a file to the Virtual SPD builder.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public void AddOrReplaceFile(string filePath)
    {
        var file = Path.GetFileName(filePath);

        switch (Path.GetExtension(file).ToLower())
        {
            case Constants.SpriteExtension:
                _customSprFiles[file] = new(filePath);
                break;
            case Constants.TextureExtension:
                _customDdsFiles[file] = new(filePath);
                break;
        }
    }

    /// <summary>
    /// Builds an SPD file.
    /// </summary>
    public unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        const int HEADER_LENGTH = 0x20;
        const int TEXTURE_ENTRY_LENGTH = 0x30;

        logger?.Info($"[{nameof(SpdBuilder)}] Building SPD File | {{0}}", filepath);

        // Get original file's entries.
        _spdHeader = GetSpdHeaderFromFile(handle, baseOffset);
        _textureEntries = GetSpdTextureEntriesFromFile(handle, baseOffset);
        _spriteEntries = GetSpdSpriteEntriesFromFile(handle, baseOffset);
        _textureData = GetSpdTextureDataFromFile(handle, baseOffset);

        // Write custom sprite entries from '.spr' files to sprite dictionary
        foreach ( var file in _customSprFiles.Values )
        {
            var stream = new FileStream(new SafeFileHandle(file.Handle, false), FileAccess.Read);
            stream.TryRead(out int spriteId, out _);
            stream.Seek(0, SeekOrigin.Begin);

            _spriteEntries[spriteId] = stream.Read<SpdSpriteEntry>();

            stream.Dispose();
        }

        // Get highest id texture entry
        int maxId = _textureEntries.Select(x => x.Key).Max();
        int nextId = maxId + 1;

        // Get DDS filenames and adjust edited sprite texture ids
        foreach ( var (key, file) in _customDdsFiles )
        {
            int newId = nextId;
            string fileName = Path.GetFileNameWithoutExtension(file.FilePath);

            if (fileName.StartsWith("spr_", StringComparison.OrdinalIgnoreCase))
            {
                
                // Remove 'spr_' in the filename and Separate Ids by '_'
                var spriteIds = fileName[4..].Split('_', StringSplitOptions.TrimEntries);

                foreach (var spriteIdStr in spriteIds)
                {
                    // Patch texture ids for each sprite id contained in the filename
                    if (int.TryParse(spriteIdStr, out int spriteId))
                    {
                        var spriteEntry = _spriteEntries[spriteId];
                        spriteEntry.SetTextureId(newId);
                        _spriteEntries[spriteId] = spriteEntry;
                    }
                }
            }
            else if(fileName.StartsWith("tex_", StringComparison.OrdinalIgnoreCase))
            {
                // Get texture id to replace from filename
                if (!int.TryParse(fileName[4..].Split("_", StringSplitOptions.TrimEntries).FirstOrDefault(), out newId))
                    continue;
            }
            else { continue; }

            // Take texture data slice and make a stream out of it
            _textureEntries[newId] = CreateTextureEntry(file, newId);
            byte[] data = new byte[file.Length];
            file.GetData(data);
            _textureData[newId] = new MemoryStream(data);

            nextId++;
        }

        MemoryStream spriteStream = BuildSpdSpriteStream();

        long textureDataOffset = HEADER_LENGTH + spriteStream.Length + (_textureEntries.Count * TEXTURE_ENTRY_LENGTH);
        MemoryStream textureEntryStream = BuildSpdTextureEntryStream(textureDataOffset, out long totalTextureSize);

        MemoryStream textureDataStream = BuildTextureDataStream((int)totalTextureSize);

        // Allocate Header
        MemoryStream headerStream = new(HEADER_LENGTH);

        // Write Header
        headerStream.Write(0x30525053); // 'SPR0'
        headerStream.Write(2); // unk04 (usually 2)

        // Calculate filesize
        long newFileSize = totalTextureSize + HEADER_LENGTH + textureEntryStream.Length + spriteStream.Length;

        headerStream.Write((int)newFileSize); // filesize
        headerStream.Write(0); // unk0c (usually 0)
        headerStream.Write(32); // unk10 (usually 32)

        int textureCount = _textureEntries.Count;
        headerStream.Write((short)textureCount); // texture count
        headerStream.Write((short)_spriteEntries.Count); // sprite count
        headerStream.Write(HEADER_LENGTH); // texture entry start offset
        headerStream.Write(HEADER_LENGTH + (textureCount * TEXTURE_ENTRY_LENGTH)); // sprite entry start offset

        // Calculate
        // Make Multistream
        var pairs = new List<StreamOffsetPair<Stream>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, HEADER_LENGTH)),

            // Add Texture Entries
            new (textureEntryStream, OffsetRange.FromStartAndLength(headerStream.Length, textureEntryStream.Length)),

            // Add Sprites
            new (spriteStream, OffsetRange.FromStartAndLength(headerStream.Length + textureEntryStream.Length, spriteStream.Length)),

            // Add Textures
            new (textureDataStream, OffsetRange.FromStartAndLength(headerStream.Length + textureEntryStream.Length + spriteStream.Length, textureDataStream.Length))
        };

        return new MultiStream(pairs, logger);
    }

    private SpdHeader GetSpdHeaderFromFile(nint handle, long pos)
    {
        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        stream.Seek(pos, SeekOrigin.Begin);

        try
        {
            return stream.Read<SpdHeader>();
        }
        finally
        {
            stream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    private SpdTextureDictionary GetSpdTextureEntriesFromFile(IntPtr handle, long pos)
    {
        SpdTextureDictionary textureDictionary = new();

        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        try
        {
            var (textureEntryCount, textureEntryOffset) = _spdHeader.GetTextureEntryCountAndOffset();

            stream.Seek(textureEntryOffset, SeekOrigin.Begin);

            for (int i = 0; i < textureEntryCount; i++)
            {
                stream.TryRead(out SpdTextureEntry entry, out _);
                textureDictionary[entry.GetTextureId()] = entry;
            }

            return textureDictionary;
        }
        finally
        {
            stream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    private SpdSpriteDictionary GetSpdSpriteEntriesFromFile(IntPtr handle, long pos)
    {
        SpdSpriteDictionary spriteDictionary = new();

        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        try
        {
            var (spriteEntryCount, spriteEntryOffset) = _spdHeader.GetSpriteEntryCountAndOffset();

            stream.Seek(spriteEntryOffset, SeekOrigin.Begin);

            for (int i = 0; i < spriteEntryCount; i++)
            {
                stream.TryRead(out SpdSpriteEntry entry, out _);
                spriteDictionary[entry.GetSpriteId()] = entry;
            }

            return spriteDictionary;
        }
        finally
        {
            stream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    private Dictionary<int, MemoryStream> GetSpdTextureDataFromFile(IntPtr handle, long pos)
    {
        // Create a dictionary to hold texture data, with the key being the texture entry's id
        Dictionary<int, MemoryStream> textureDataDictionary = new();

        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        try
        {
            foreach (var entry in _textureEntries.Values)
            {
                var (offset, size) = entry.GetTextureOffsetAndSize();
                byte[] data = new byte[size];
                stream.Seek(offset, SeekOrigin.Begin);
                stream.TryRead(data, out _);
                textureDataDictionary[entry.GetTextureId()] = new MemoryStream(data);
            }

            return textureDataDictionary;
        }
        finally
        {
            stream.Dispose();
            _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
    }

    /// <summary>
    /// Writes SPD texture entries to a stream.
    /// </summary>
    /// <param name="textureDataOffset">Where in the file the actual textures will be written. Will be used to write pointers to the texture data.</param>
    /// <param name="totalTextureSize">Returns the total byte size of all textures.</param>
    private MemoryStream BuildSpdTextureEntryStream(long textureDataOffset, out long totalTextureSize)
    {
        const int TEXTURE_DATA_ENTRY_SIZE = 0x30;

        totalTextureSize = 0;
        MemoryStream stream = new(TEXTURE_DATA_ENTRY_SIZE * (_textureEntries.Count + _textureData.Count));

        // Write existing texture entries to the stream
        foreach (var texture in _textureEntries.Values)
        {
            texture.SetTextureOffset((int)textureDataOffset);
            stream.Write(texture); // Write texture entry
            var (offset, size) = texture.GetTextureOffsetAndSize();
            textureDataOffset += size; // move new offset to end of previous texture
            totalTextureSize += size;
        }

        return stream;
    }


    /// <summary>
    /// Writes SPD sprite entries to a stream.
    /// </summary>
    private MemoryStream BuildSpdSpriteStream()
    {
        const int SPRITE_ENTRY_SIZE = 0xa0;

        MemoryStream stream = new(_spriteEntries.Count * SPRITE_ENTRY_SIZE);

        foreach(var sprite in _spriteEntries.Values)
        {
            stream.Write(sprite);
        }

        return stream;
    }

    /// <summary>
    /// Writes raw textures to a stream.
    /// </summary>
    /// <param name="handle">Handle for the SPD file to get texture data from.</param>
    /// <param name="streamSize">The byte size of all textures combined.</param>
    private MemoryStream BuildTextureDataStream(int streamSize)
    {
        // data stream
        MemoryStream stream = new(streamSize);

        // Write original textures
        foreach (var texture in _textureData.Values)
        {
            texture.CopyTo(stream);
        }

        return stream;
    }

    /// <summary>
    /// Create an spd texture entry using information from a dds file.
    /// <param name="texture">The data slice of the texture to be read.</param>
    /// <param name="id">The Id of the new texture.</param>
    /// </summary>
    public SpdTextureEntry CreateTextureEntry(FileSlice texture, int id)
    {
        long fileSize = texture.Length;

        var textureEntryStream = new MemoryStream(0x30);

        var name = Encoding.ASCII.GetBytes($"texture_{id}".PadRight(16, '\0').ToCharArray());

        var ddsSlice = texture.SliceUpTo(0xc, 8);
        ddsSlice.GetData(out byte[] ddsDimensions);
        var ddsStream = new MemoryStream(ddsDimensions);

        ddsStream.TryRead(out uint textureHeight, out _);
        ddsStream.TryRead(out uint textureWidth, out _);

        textureEntryStream.Write(id); // texture id
        textureEntryStream.Write(0); // unk04
        textureEntryStream.Write(0); // texture data pointer (set to 0 now, will be changed when being written to file)
        textureEntryStream.Write((int)fileSize); // dds filesize
        textureEntryStream.Write(textureWidth); // dds width
        textureEntryStream.Write(textureHeight); // dds height
        textureEntryStream.Write(0); // unk18
        textureEntryStream.Write(0); // unk1c
        textureEntryStream.Write(name);

        textureEntryStream.Seek(0, SeekOrigin.Begin);
        var entry = textureEntryStream.Read<SpdTextureEntry>();

        return entry;
    }
}
