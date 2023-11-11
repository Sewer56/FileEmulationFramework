using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.Extensions;
using SPD.File.Emulator.Sprite;
using System.Runtime.InteropServices;
using System.Text;

namespace SPD.File.Emulator.Spd;

public class SpdBuilder : SpriteBuilder
{
    private Dictionary<int, SpdTextureEntry> _textureEntries = new();
    private Dictionary<int, Stream> _textureData = new();
    private Dictionary<int, SpdSpriteEntry> _spriteEntries = new();
    private Dictionary<int, SpdSpriteEntry> _newSpriteEntries = new();

    private SpdHeader _spdHeader;
    public SpdBuilder(Logger log) : base(log) { }

    public override void AddOrReplaceFile(string filePath)
    {
        if (filePath.EndsWith(Constants.SpdSpriteExtension, StringComparison.OrdinalIgnoreCase))
            CustomSprFiles[filePath] = new(filePath);
        else if (filePath.EndsWith(Constants.SpdTextureExtension, StringComparison.OrdinalIgnoreCase))
            CustomTextureFiles[filePath] = new(filePath);
    }

    /// <summary>
    /// Builds an SPD file.
    /// </summary>
    public override unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0)
    {
        const int headerLength = 0x20;
        const int textureEntryLength = 0x30;

        logger?.Info($"[{nameof(SpdBuilder)}] Building SPD File | {{0}}", filepath);

        // Get original file's entries.
        var spdFileSlice = new FileSlice(filepath);
        var spdStream = new FileSliceStreamW32(spdFileSlice);
        _spdHeader = GetHeaderFromSpr<SpdHeader>(spdStream, baseOffset);
        _textureEntries = GetTextureEntriesFromFile(spdStream, baseOffset);
        _spriteEntries = GetSpriteEntriesFromFile(spdStream, baseOffset);
        _textureData = GetTextureDataFromFile(spdFileSlice, baseOffset);
        spdStream.Dispose();

        // Write custom sprite entries from '.spdspr' files to sprite dictionary
        foreach ( var file in CustomSprFiles.Values )
        {
            using var stream = new FileSliceStreamFs(file);
            stream.TryRead(out int spriteId, out _);
            stream.Seek(0, SeekOrigin.Begin);

            _newSpriteEntries[spriteId] = stream.Read<SpdSpriteEntry>();
        }

        // Get highest id texture entry
        int maxId = _textureEntries.Select(x => x.Key).Max();
        int nextId = maxId + 1;

        // Create empty HashSet to use for texture names with no exclude separator '~' to reduce allocations
        HashSet<int> emptyHashSet = new();

        // Get DDS filenames and adjust edited sprite texture ids
        foreach ( var (key, file) in CustomTextureFiles )
        {
            int newId = nextId;
            string fileName = Path.GetFileNameWithoutExtension(file.FilePath);

            if (fileName.StartsWith("spr_", StringComparison.OrdinalIgnoreCase))
            {
                foreach(int id in GetSpriteIdsFromFilename(fileName[4..]))
                {
                    // Check for accompanying .spdspr file
                    if (!CustomSprFiles.ContainsKey(Path.GetDirectoryName(key) + $"\\spr_{id}{Constants.SpdSpriteExtension}"))
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

                if (!int.TryParse(ids[0], out int texId)) continue;

                // Get sprite ids to preserve
                HashSet<int> excludeIds = emptyHashSet;
                if (ids.Length > 1)
                    excludeIds = GetSpriteIdsFromFilename(ids[1]);

                // Revert each modified sprite that used to point to the texture, then patch them to point to the new one
                foreach (var (spriteId, sprite) in _spriteEntries)
                {
                    if (sprite.GetSpriteTextureId() == texId && !excludeIds.Contains(spriteId))
                    {
                        _newSpriteEntries[spriteId] = sprite;
                        PatchSpriteEntry(spriteId, newId);
                    }
                }
            }
            else continue;

            // Take texture data slice and make a stream out of it
            _textureEntries[newId] = CreateTextureEntry(file, newId);
            _textureData[newId] = new FileSliceStreamW32(file);

            nextId++;
        }

        // Copy new sprite entries into the original sprite entry list
        foreach(var (key, value) in _newSpriteEntries)
        {
            _spriteEntries[key] = value;
        }

        MemoryStream spriteStream = BuildSpdSpriteStream();

        long textureDataOffset = headerLength + spriteStream.Length + (_textureEntries.Count * textureEntryLength);
        MemoryStream textureEntryStream = BuildSpdTextureEntryStream(textureDataOffset, out long totalTextureSize);

        // Allocate Header
        MemoryStream headerStream = new(headerLength);

        // Write Header

        // Calculate filesize
        long newFileSize = totalTextureSize + headerLength + textureEntryStream.Length + spriteStream.Length;

        _spdHeader._fileSize = (int)newFileSize;
        _spdHeader._textureEntryCount = (short)_textureEntries.Count;
        _spdHeader._spriteEntryCount = (short)_spriteEntries.Count;
        _spdHeader._textureEntryOffset = headerLength;
        _spdHeader._spriteEntryOffset = headerLength + (_textureEntries.Count * textureEntryLength);

        headerStream.Write(_spdHeader);

        // Make Multistream
        var pairs = new List<StreamOffsetPair<Stream>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, headerLength)),
        
            // Add Texture Entries
            new (textureEntryStream, OffsetRange.FromStartAndLength(headerStream.Length, textureEntryStream.Length)),
        
            // Add Sprites
            new (spriteStream, OffsetRange.FromStartAndLength(headerStream.Length + textureEntryStream.Length, spriteStream.Length))
        };

        // Add Textures
        long currentMultiStreamLength = headerStream.Length + textureEntryStream.Length + spriteStream.Length;
        foreach (var texture in _textureData.Values)
        {
            pairs.Add(new StreamOffsetPair<Stream>(texture, OffsetRange.FromStartAndLength(currentMultiStreamLength, texture.Length)));
            currentMultiStreamLength += texture.Length;
        }

        return new MultiStream(pairs, logger);
    }

    private void PatchSpriteEntry(int spriteId, int newTextureId)
    {
        if (!_newSpriteEntries.ContainsKey(spriteId))
        {
            _log.Error("Tried to patch non-existent SPD id {0}. Skipping...", spriteId);
            return;
        }

        CollectionsMarshal.GetValueRefOrNullRef(_newSpriteEntries, spriteId).SetTextureId(newTextureId);
    }

    private Dictionary<int, SpdTextureEntry> GetTextureEntriesFromFile(Stream stream, long pos)
    {
        Dictionary<int, SpdTextureEntry> textureDictionary = new();

        var (textureEntryCount, textureEntryOffset) = _spdHeader.GetTextureEntryCountAndOffset();

        stream.Seek(textureEntryOffset, SeekOrigin.Begin);

        for (int i = 0; i < textureEntryCount; i++)
        {
            stream.TryRead(out SpdTextureEntry entry, out _);
            textureDictionary[entry.GetTextureId()] = entry;
        }

        return textureDictionary;
    }

    private Dictionary<int, SpdSpriteEntry> GetSpriteEntriesFromFile(Stream stream, long pos)
    {
        Dictionary<int, SpdSpriteEntry> spriteDictionary = new();

        var (spriteEntryCount, spriteEntryOffset) = _spdHeader.GetSpriteEntryCountAndOffset();

        stream.Seek(spriteEntryOffset, SeekOrigin.Begin);

        for (int i = 0; i < spriteEntryCount; i++)
        {
            stream.TryRead(out SpdSpriteEntry entry, out _);
            spriteDictionary[entry.GetSpriteId()] = entry;
        }

        return spriteDictionary;
    }

    private Dictionary<int, Stream> GetTextureDataFromFile(FileSlice spdSlice, long pos)
    {
        // Create a dictionary to hold texture data, with the key being the texture entry's id
        Dictionary<int, Stream> textureDataDictionary = new();

        foreach (var entry in _textureEntries.Values)
        {
            var (offset, size) = entry.GetTextureOffsetAndSize();
            textureDataDictionary[entry.GetTextureId()] = new FileSliceStreamW32(spdSlice.Slice(offset, size));
        }

        return textureDataDictionary;
    }

    /// <summary>
    /// Writes SPD texture entries to a stream.
    /// </summary>
    /// <param name="textureDataOffset">Where in the file the actual textures will be written. Will be used to write pointers to the texture data.</param>
    /// <param name="totalTextureSize">Returns the total byte size of all textures.</param>
    private MemoryStream BuildSpdTextureEntryStream(long textureDataOffset, out long totalTextureSize)
    {
        const int textureDataEntrySize = 0x30;

        totalTextureSize = 0;
        MemoryStream stream = new(textureDataEntrySize * (_textureEntries.Count + _textureData.Count));

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
        const int spriteEntrySize = 0xa0;

        MemoryStream stream = new(_spriteEntries.Count * spriteEntrySize);

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