using SPD.File.Emulator.Sprite;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Extensions;
using System.Text;
using System.Runtime.InteropServices;

namespace SPD.File.Emulator.Spd;

public class SpdBuilder : SpriteBuilder
{
    private SpdTextureDictionary _textureEntries = new();
    private Dictionary<int, MemoryStream> _textureData = new();
    private SpdSpriteDictionary _spriteEntries = new();
    private SpdSpriteDictionary _newSpriteEntries = new();

    private SpdHeader _spdHeader;
    public SpdBuilder(Logger log) : base(log) { }

    public override void AddOrReplaceFile(string filePath)
    {
        var file = Path.GetFileName(filePath);

        switch (Path.GetExtension(file).ToLower())
        {
            case Constants.SpdSpriteExtension:
                _customSprFiles[filePath] = new(filePath);
                break;
            case Constants.SpdTextureExtension:
                _customTextureFiles[filePath] = new(filePath);
                break;
        }
    }

    /// <summary>
    /// Builds an SPD file.
    /// </summary>
    public override unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        foreach (var spr in _customSprFiles.Keys)
        {
            _log.Info("boobs {0}", spr);
        }


        const int HEADER_LENGTH = 0x20;
        const int TEXTURE_ENTRY_LENGTH = 0x30;

        logger?.Info($"[{nameof(SpdBuilder)}] Building SPD File | {{0}}", filepath);

        // Get original file's entries.
        _spdHeader = GetHeaderFromSpr<SpdHeader>(handle, baseOffset);
        _textureEntries = GetTextureEntriesFromFile(handle, baseOffset);
        _spriteEntries = GetSpriteEntriesFromFile(handle, baseOffset);
        _textureData = GetTextureDataFromFile(handle, baseOffset);

        // Write custom sprite entries from '.spdspr' files to sprite dictionary
        foreach ( var file in _customSprFiles.Values )
        {
            var stream = new FileStream(new SafeFileHandle(file.Handle, false), FileAccess.Read);
            stream.TryRead(out int spriteId, out _);
            stream.Seek(0, SeekOrigin.Begin);

            _newSpriteEntries[spriteId] = stream.Read<SpdSpriteEntry>();

            stream.Dispose();
        }

        // Get highest id texture entry
        int maxId = _textureEntries.Select(x => x.Key).Max();
        int nextId = maxId + 1;

        // Get DDS filenames and adjust edited sprite texture ids
        foreach ( var (key, file) in _customTextureFiles )
        {
            int newId = nextId;
            string fileName = Path.GetFileNameWithoutExtension(file.FilePath);

            if (fileName.StartsWith("spr_", StringComparison.OrdinalIgnoreCase))
            {
                foreach(int id in GetSpriteIdsFromFilename(fileName))
                {
                    // Check for accompanying .spdspr file
                    if (!_customSprFiles.ContainsKey(Path.GetDirectoryName(key) + $"\\spr_{id}{Constants.SpdSpriteExtension}"))
                    {
                        if (_spriteEntries.TryGetValue(id, out var _newSpriteEntry))
                        {
                            _newSpriteEntries[id] = _spriteEntries[id];
                        }
                    }

                    PatchSpriteEntry(id, newId);
                }
            }
            else if(fileName.StartsWith("tex_", StringComparison.OrdinalIgnoreCase))
            {
                string[] ids = fileName[4..].Split('~');
                // Get texture id to replace from filename

                if (!int.TryParse(ids[0], out int tex_id)) continue;

                // Get sprite ids to preserve
                List<int> exclude_ids = new();
                if (ids.Length > 1)
                {
                    exclude_ids = GetSpriteIdsFromFilename("tex_" + ids[1]);
                }

                // Revert each modified sprite that used to point to the textures
                foreach (var (spriteId, sprite) in _spriteEntries)
                {
                    if (sprite.GetSpriteTextureId() == tex_id && !exclude_ids.Contains(spriteId))
                    {
                        _newSpriteEntries[spriteId] = sprite;
                        PatchSpriteEntry(spriteId, newId);
                    }
                }
            }
            else continue;

            // Take texture data slice and make a stream out of it
            _textureEntries[newId] = CreateTextureEntry(file, newId);
            byte[] data = new byte[file.Length];
            file.GetData(data);
            _textureData[newId] = new MemoryStream(data);

            nextId++;
        }

        // Copy new sprite entries into the original sprite entry list
        foreach(var (key, value) in _newSpriteEntries)
        {
            _spriteEntries[key] = value;
        }

        MemoryStream spriteStream = BuildSpdSpriteStream();

        long textureDataOffset = HEADER_LENGTH + spriteStream.Length + (_textureEntries.Count * TEXTURE_ENTRY_LENGTH);
        MemoryStream textureEntryStream = BuildSpdTextureEntryStream(textureDataOffset, out long totalTextureSize);

        MemoryStream textureDataStream = BuildTextureDataStream((int)totalTextureSize);

        // Allocate Header
        MemoryStream headerStream = new(HEADER_LENGTH);

        // Write Header

        // Calculate filesize
        long newFileSize = totalTextureSize + HEADER_LENGTH + textureEntryStream.Length + spriteStream.Length;

        _spdHeader._fileSize = (int)newFileSize;
        _spdHeader._textureEntryCount = (short)_textureEntries.Count;
        _spdHeader._spriteEntryCount = (short)_spriteEntries.Count;
        _spdHeader._textureEntryOffset = HEADER_LENGTH;
        _spdHeader._spriteEntryOffset = HEADER_LENGTH + (_textureEntries.Count * TEXTURE_ENTRY_LENGTH);

        headerStream.Write(_spdHeader);

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

    private void PatchSpriteEntry(int spriteId, int newTextureId)
    {
        if (!_newSpriteEntries.ContainsKey(spriteId))
        {
            _log.Error("Tried to patch non-existent SPD id {0}. Skipping...", spriteId);
            return;
        }

        _newSpriteEntries[spriteId] = _newSpriteEntries[spriteId].SetTextureId(newTextureId);
    }

    private SpdTextureDictionary GetTextureEntriesFromFile(IntPtr handle, long pos)
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

    private SpdSpriteDictionary GetSpriteEntriesFromFile(IntPtr handle, long pos)
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

    private Dictionary<int, MemoryStream> GetTextureDataFromFile(IntPtr handle, long pos)
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
    public static SpdTextureEntry CreateTextureEntry(FileSlice texture, int id)
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