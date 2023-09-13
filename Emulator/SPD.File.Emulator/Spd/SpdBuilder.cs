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
using SPD.File.Emulator;
using SPD.File.Emulator.Utilities;

namespace SPD.File.Emulator.Spd;

public class SpdBuilder
{
    private Dictionary<string, FileSlice> _customSprFiles = new();
    private Dictionary<string, FileSlice> _customSpdTexFiles = new();
    private Dictionary<string, FileSlice> _customDdsFiles = new();
    private SpdTextureDictionary _textureEntries = new();
    private SpdSpriteDictionary _spriteEntries = new();

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
        var nameIndex = filePath.IndexOf(filePath.Replace('\\', '/'), StringComparison.Ordinal);
        var file = Path.GetFileName(filePath);

        switch (Path.GetExtension(file).ToLower())
        {
            case Constants.SpriteExtension:
                _customSprFiles[file] = new(filePath);
                break;
            case Constants.TextureEntryExtension:
                _customSpdTexFiles[file] = new(filePath);
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
        _textureEntries = GetSpdTextureEntriesFromFile(handle, baseOffset);
        _spriteEntries = GetSpdSpriteEntriesFromFile(handle, baseOffset);

        // Overwrite sprite entries with spr entries
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

        // Get DDS filenames and adjust edited sprite texture ids
        foreach ( var file in _customDdsFiles.Values )
        {
            // Separate Ids in the filename by '_'
            var spriteIds = Path.GetFileNameWithoutExtension(file.FilePath).Split('_', StringSplitOptions.TrimEntries);

            foreach ( var spriteIdStr in spriteIds )
            {
                // Patch texture ids for each sprite id contained in the filename
                if (int.TryParse(spriteIdStr, out int spriteId))
                {
                    var spriteEntry = _spriteEntries[spriteId];
                    spriteEntry.SetTextureId(maxId + 1);
                    _spriteEntries[spriteId] = spriteEntry;
                }
            }

            maxId++;
        }

        MemoryStream spriteStream = BuildSpdSpriteStream();

        long textureDataOffset = HEADER_LENGTH + spriteStream.Length + ((_customDdsFiles.Count + _textureEntries.Count) * TEXTURE_ENTRY_LENGTH);
        MemoryStream textureEntryStream = BuildSpdTextureEntryStream(textureDataOffset, maxId - _customDdsFiles.Count, out long totalTextureSize);

        MemoryStream textureDataStream = BuildTextureDataStream(handle, (int)totalTextureSize);

        // Allocate Header
        MemoryStream headerStream = new(HEADER_LENGTH);

        // Write Header
        headerStream.Write(0x30525053); // 'SPR0'
        headerStream.Write(2); // unk04

        // Calculate filesize
        long newFileSize = totalTextureSize + HEADER_LENGTH + textureEntryStream.Length + spriteStream.Length;

        headerStream.Write((int)newFileSize); // filesize
        headerStream.Write(0); // unk0c
        headerStream.Write(32); // unk10

        int textureCount = _textureEntries.Count + _customDdsFiles.Count;
        headerStream.Write((short)textureCount); // texture count
        headerStream.Write((short)_spriteEntries.Count); // sprite count
        headerStream.Write(HEADER_LENGTH); // texture entry start offset
        headerStream.Write(HEADER_LENGTH + (textureCount * 0x30)); // sprite entry start offset

        // Calculate
        // Make Multistream
        var pairs = new List<StreamOffsetPair<Stream>>()
        {
            // Add Header
            new (headerStream, OffsetRange.FromStartAndLength(0, HEADER_LENGTH)),

            // Add Textures
            new (textureEntryStream, OffsetRange.FromStartAndLength(headerStream.Length, textureEntryStream.Length)),

            // Add Sprites
            new (spriteStream, OffsetRange.FromStartAndLength(headerStream.Length + textureEntryStream.Length, spriteStream.Length)),

            // Add Sprites
            new (textureDataStream, OffsetRange.FromStartAndLength(headerStream.Length + textureEntryStream.Length + spriteStream.Length, textureDataStream.Length))
        };

        // Merge the slices and add.
        //var mergeAbleStreams = new List<StreamOffsetPair<Stream>>(); 
        //foreach (var merged in FileSliceStreamExtensions.MergeStreams(mergeAbleStreams))
        //    pairs.Add(merged);

        return new MultiStream(pairs, logger);
    }


    /// <summary>
    /// Writes raw textures to a stream.
    /// </summary>
    /// <param name="handle">Handle for the SPD file to get texture data from.</param>
    /// <param name="streamSize">The byte size of all textures combined.</param>
    private MemoryStream BuildTextureDataStream(IntPtr handle, int streamSize)
    {
        // data stream
        MemoryStream stream = new(streamSize);
        var spdFileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        foreach (var entry in _textureEntries.Values)
        {
            var (offset, size) = entry.GetTextureOffsetAndSize();
            spdFileStream.Seek(offset, SeekOrigin.Begin);

            byte[] data = new byte[size];
            spdFileStream.Read(data, 0, size);

            stream.Write(data);
        }

        foreach (var entry in _customDdsFiles.Values)
        {
            byte[] data = new byte[entry.Length];
            entry.GetData(data);
            stream.Write(data);
        }

        spdFileStream.Dispose();
        return stream;
    }


    /// <summary>
    /// Writes SPD sprite entries to a stream.
    /// </summary>
    private MemoryStream BuildSpdSpriteStream()
    {
        MemoryStream stream = new(_spriteEntries.Count * 160);

        foreach(var sprite in _spriteEntries.Values)
        {
            stream.Write(sprite);
        }

        return stream;
    }

    /// <summary>
    /// Writes SPD texture entries to a stream.
    /// </summary>
    /// <param name="textureDataOffset">Where in the file the actual textures will be written. Will be used to write pointers to the texture data.</param>
    /// <param name="maxId">The largest texture id before adding new files. Used to determine the value of new ids.</param>
    /// <param name="totalTextureSize">Returns the total byte size of all textures.</param>
    private MemoryStream BuildSpdTextureEntryStream(long textureDataOffset, int maxId, out long totalTextureSize)
    {
        const int TEXTURE_DATA_ENTRY_SIZE = 0x30;

        totalTextureSize = 0;
        MemoryStream stream = new(TEXTURE_DATA_ENTRY_SIZE * (_textureEntries.Count + _customDdsFiles.Count));

        // Write existing texture entries to the stream
        foreach (var texture in _textureEntries.Values)
        {
            texture.SetTextureOffset((int)textureDataOffset);
            stream.Write(texture); // Write texture entry
            var (offset, size) = texture.GetTextureOffsetAndSize();
            textureDataOffset += size; // move new offset to end of previous texture
            totalTextureSize += size;
        }

        // Generate new entries based on dds metadata
        foreach (var texture in _customDdsFiles.Values)
        {
            maxId++;

            long fileSize = texture.Length;

            byte[] nameBuffer = new byte[16];
            nameBuffer = Encoding.ASCII.GetBytes($"texture_{maxId}".PadRight(16, '\0').ToCharArray());

            for(int i = 0; i < nameBuffer.Length; i++)
            {
                if (nameBuffer[i] == 0)
                    nameBuffer[i] = 0;
            }

            var ddsSlice = texture.SliceUpTo(0xc, 8);
            ddsSlice.GetData(out byte[] ddsDimensions);
            var ddsStream = new MemoryStream(ddsDimensions);

            ddsStream.TryRead(out uint textureWidth, out _);
            ddsStream.TryRead(out uint textureHeight, out _);

            stream.Write(maxId); // texture id
            stream.Write(0); // unk04
            stream.Write((int)textureDataOffset); // texture data pointer
            stream.Write((int)fileSize); // dds filesize
            stream.Write(textureWidth); // dds width
            stream.Write(textureHeight); // dds height
            stream.Write(0); // unk18
            stream.Write(0); // unk1c
            stream.Write(nameBuffer);

            totalTextureSize += fileSize;
        }

        return stream;
    }
    private SpdTextureDictionary GetSpdTextureEntriesFromFile(IntPtr handle, long pos)
    {
        SpdTextureDictionary textureDictionary = new();

        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        try
        {
            //Get the amount of texture entries
            stream.Seek(pos + 0x14, SeekOrigin.Begin);
            stream.TryRead(out short textureEntryCount, out _);

            //Get the offset where texture entries start
            stream.Seek(pos + 0x18, SeekOrigin.Begin);
            stream.TryRead(out int textureEntryOffset, out _);

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
        const int SPRITE_ENTRY_SIZE = 0xa0;

        SpdSpriteDictionary spriteDictionary = new();

        var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

        try
        {
            //Get the amount of sprite entries
            stream.Seek(pos + 0x16, SeekOrigin.Begin);
            stream.TryRead(out short spriteEntryCount, out _);

            //Get the offset where sprite entries start
            stream.Seek(pos + 0x1c, SeekOrigin.Begin);
            stream.TryRead(out int spriteEntryOffset, out _);

            //var entries = GC.AllocateUninitializedArray<SpdSpriteEntry>(spriteEntryCount);
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
}
