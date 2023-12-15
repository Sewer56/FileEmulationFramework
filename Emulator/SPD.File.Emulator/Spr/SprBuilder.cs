using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.Extensions;
using SPD.File.Emulator.Sprite;
using System.Runtime.InteropServices;

namespace SPD.File.Emulator.Spr;

public class SprBuilder : SpriteBuilder
{
    private List<Stream> _textureData = new();
    private List<SprSpriteEntry> _spriteEntries = new();
    private Dictionary<int, SprSpriteEntry> _newSpriteEntries = new();

    private SprHeader _sprHeader;
    private int _totalTextureSize;

    public SprBuilder() { }
    public SprBuilder(Logger log) : base(log) { }

    public override void AddOrReplaceFile(string filePath)
    {
        if (filePath.EndsWith(Constants.SprSpriteExtension, StringComparison.OrdinalIgnoreCase))
            CustomSprFiles[filePath] = new(filePath);
        else if (filePath.EndsWith(Constants.SprTextureExtension, StringComparison.OrdinalIgnoreCase))
            CustomTextureFiles[filePath] = new(filePath);
    }

    public override MultiStream Build(string filepath, Logger? logger = null, long baseOffset = 0)
    {
        const int headerLength = 0x20;
        const int pointerEntryLength = 0x8;

        logger?.Info($"[{nameof(SprBuilder)}] Building SPR File | {{0}}", filepath);

        // Get original file's entries.
        var sprSlice = new FileSlice(filepath);
        var sprStream = new FileSliceStreamW32(sprSlice);
        _sprHeader = GetHeaderFromSpr<SprHeader>(sprStream, baseOffset);
        GetTextureDataFromSpr(sprSlice, sprStream);
        GetSpriteEntriesFromSpr(sprStream);
        sprStream.Dispose();

        int nextId = _textureData.Count;

        // Create empty HashSet to use for texture names with no exclude separator '~' to reduce allocations
        HashSet<int> emptyHashSet = new();
        var textureSeparatedSpriteDict = CreateTextureSeparatedSpriteDict();

        // Get DDS filenames and adjust edited sprite texture ids
        foreach ((string key, var file) in CustomTextureFiles)
        {
            int newId = nextId;
            string fileName = Path.GetFileNameWithoutExtension(file.FilePath);

            if (fileName.StartsWith("spr_", StringComparison.OrdinalIgnoreCase))
            {
                foreach (int id in GetSpriteIdsFromFilename(fileName[4..]))
                {
                    string spriteEntryPath = Path.Combine(Path.GetDirectoryName(key)!, $"spr_{id}{Constants.SprSpriteExtension}");

                    // Get accompanying sprite entry if it exists. if not, use original sprite entry data
                    if (CustomSprFiles.TryGetValue(spriteEntryPath, out var customSprFile))
                    {
                        using var stream = new FileSliceStreamW32(customSprFile);
                        _newSpriteEntries[id] = stream.Read<SprSpriteEntry>();
                    }
                    else
                    {
                        if (id < _spriteEntries.Count)
                        {
                            _newSpriteEntries[id] = _spriteEntries[id];
                        }
                    }

                    PatchSpriteEntry(id, newId);
                }
            }
            else if (fileName.StartsWith("tex_", StringComparison.OrdinalIgnoreCase))
            {
                // Get texture id to replace from filename
                string[] ids = fileName[4..].Split('~');

                if (!int.TryParse(ids[0], out int texId)) continue;

                // Get sprite ids to preserve
                HashSet<int> excludeIds;
                if (ids.Length > 1)
                    excludeIds = GetSpriteIdsFromFilename(ids[1]);
                else
                    excludeIds = emptyHashSet;

                // Revert each modified sprite that used to point to the textures then patch them to point to the new one
                if (textureSeparatedSpriteDict.TryGetValue(texId, out var sprites))
                {
                    foreach ((int index, var sprite) in sprites)
                    {
                        if (!excludeIds.Contains(index))
                        {
                            _newSpriteEntries[index] = sprite;
                            PatchSpriteEntry(index, newId);
                        }
                    }
                }
            }
            else continue;

            _textureData.Add(new FileSliceStreamW32(file));

            nextId++;
        }


        // Copy new sprite entries into the original sprite entry list
        foreach ((int index, _) in _newSpriteEntries)
        {
            if (index < _spriteEntries.Count)
                _spriteEntries[index] = _newSpriteEntries[index];
            else
            {
                // Add dummy spr entries up to current spr
                _spriteEntries.AddRange(new SprSpriteEntry[index - _spriteEntries.Count]);

                _spriteEntries.Add(_newSpriteEntries[index]);
            }
        }

        var pointerStream = BuildPointerList();
        var spriteStream = BuildSpriteStream();

        // Allocate Header
        MemoryStream headerStream = new(headerLength);

        // Write Header

        // Calculate filesize
        long newFileSize = headerLength + pointerStream.Length + spriteStream.Length + _totalTextureSize;

        _sprHeader.FileSize = (int)newFileSize;
        _sprHeader.TextureEntryCount = (short)_textureData.Count;
        _sprHeader.SpriteEntryCount = (short)_spriteEntries.Count;
        _sprHeader.TextureEntryOffset = headerLength;
        _sprHeader.SpriteEntryOffset = headerLength + (_textureData.Count * pointerEntryLength);

        headerStream.Write(_sprHeader);

        // Calculate
        // Make Multistream
        var pairs = new List<StreamOffsetPair<Stream>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, headerLength)),

            // Add Pointer Entries
            new (pointerStream, OffsetRange.FromStartAndLength(headerLength, pointerStream.Length)),

            // Add Sprites
            new (spriteStream, OffsetRange.FromStartAndLength(headerLength + pointerStream.Length, spriteStream.Length)),
        };

        // Add Textures
        long currentMultiStreamLength = headerLength + pointerStream.Length + spriteStream.Length;
        foreach (var texture in _textureData)
        {
            pairs.Add(new StreamOffsetPair<Stream>(texture, OffsetRange.FromStartAndLength(currentMultiStreamLength, texture.Length)));
            currentMultiStreamLength += texture.Length;
        }

        return new MultiStream(pairs, logger);
    }

    /// <summary>
    /// Writes SPR pointer list to a stream.
    /// </summary>
    private MemoryStream BuildPointerList()
    {
        // Constants
        const int headerSize = 0x20;
        const int pointerEntrySize = 0x8;
        const int spriteEntrySize = 0x80;

        // Calculate pointer list sizes
        int pointerEntryListSize = (_spriteEntries.Count + _textureData.Count) * pointerEntrySize;
        int spriteEntryListSize = spriteEntrySize * _spriteEntries.Count;

        int paddingSize = pointerEntryListSize % 0x10;
        pointerEntryListSize += paddingSize;

        MemoryStream stream = new(pointerEntryListSize);

        // Calculate the starting offsets of the sprite and texture listss
        int spriteEntryOffset = headerSize + pointerEntryListSize;
        int textureEntryOffset = spriteEntryOffset + spriteEntryListSize;

        // Write texture pointers
        foreach (var entry in _textureData)
        {
            stream.Write(new SprPointer(textureEntryOffset));
            int fileSize = (int)entry.Length;
            textureEntryOffset += fileSize;
            _totalTextureSize += fileSize;
        }

        // Write sprite pointers
        for (int i = 0; i < _spriteEntries.Count; i++)
        {
            stream.Write(new SprPointer(spriteEntryOffset));
            spriteEntryOffset += spriteEntrySize;
        }

        byte[] paddingBytes = new byte[paddingSize];
        stream.Write(paddingBytes);

        return stream;
    }

    /// <summary>
    /// Writes SPR sprite entries to a stream.
    /// </summary>
    private MemoryStream BuildSpriteStream()
    {
        const int spriteEntrySize = 0x80;

        MemoryStream stream = new(_spriteEntries.Count * spriteEntrySize);

        foreach (var sprite in _spriteEntries)
        {
            stream.Write(sprite);
        }

        return stream;
    }

    /// <summary>
    /// Changes the texture Id a sprite points to.
    /// </summary>
    private void PatchSpriteEntry(int spriteId, int newTextureId)
    {
        if (!_newSpriteEntries.ContainsKey(spriteId))
        {
            Log?.Error("Tried to patch non-existent SPR id {0}. Skipping...", spriteId);
            return;
        }

        CollectionsMarshal.GetValueRefOrNullRef(_newSpriteEntries, spriteId).SetTextureId(newTextureId);
    }

    private void GetTextureDataFromSpr(FileSlice sprSlice, FileSliceStreamW32 stream)
    {
        (short count, int offset) = _sprHeader.GetTextureEntryCountAndOffset();
        stream.Seek(offset, SeekOrigin.Begin);

        for (int i = 0; i < count; i++)
        {
            var pointer = stream.Read<SprPointer>();
            _textureData.Add(ReadTmxFromSpr(sprSlice, stream, pointer.GetOffset()));
        }
    }

    private void GetSpriteEntriesFromSpr(Stream stream)
    {
        (short count, int offset) = _sprHeader.GetSpriteEntryCountAndOffset();
        stream.Seek(offset, SeekOrigin.Begin);

        for (int i = 0; i < count; i++)
        {
            var pointer = stream.Read<SprPointer>();
            _spriteEntries.Add(ReadSprite(stream, pointer.GetOffset()));
        }
    }

    /// <summary>
    /// Changes the texture Id a sprite points to.
    /// </summary>
    private static Stream ReadTmxFromSpr(FileSlice sprSlice, FileSliceStreamW32 stream, long tmxOffset)
    {
        // Hold current stream position
        long pos = stream.Position;

        // Seek tmx offset in spr
        stream.Seek(tmxOffset, SeekOrigin.Begin);

        // Read tmx size from tmx
        stream.Read<int>();
        int tmxSize = stream.Read<int>();

        // Return stream to the original position
        stream.Seek(pos, SeekOrigin.Begin);

        return new FileSliceStreamW32(sprSlice.Slice(tmxOffset, tmxSize));
    }

    private static SprSpriteEntry ReadSprite(Stream stream, long spriteOffset)
    {
        long pos = stream.Position;

        stream.Seek(spriteOffset, SeekOrigin.Begin);
        var sprite = stream.Read<SprSpriteEntry>();
        stream.Seek(pos, SeekOrigin.Begin);

        return sprite;
    }

    /// <summary>
    /// Returns a dictionary with the sprite ids separated by texture id.
    /// </summary>
    private Dictionary<int, Dictionary<int, SprSpriteEntry>> CreateTextureSeparatedSpriteDict()
    {
        var resultDict = new Dictionary<int, Dictionary<int, SprSpriteEntry>>();

        for (int i = 0; i < _spriteEntries.Count; i++)
        {
            var sprite = _spriteEntries[i];
            int textureId = sprite.GetSpriteTextureId();

            if (resultDict.TryGetValue(textureId, out var sprites))
            {
                sprites[i] = sprite;
            }
            else
            {
                resultDict[textureId] = new Dictionary<int, SprSpriteEntry>
                {
                    { i, sprite }
                };
            }
        }

        return resultDict;
    }
}
