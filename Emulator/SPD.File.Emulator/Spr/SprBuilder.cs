using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Extensions;
using SPD.File.Emulator.Spd;
using SPD.File.Emulator.Sprite;
using System.IO;
using System;
using System.Linq;

namespace SPD.File.Emulator.Spr
{
    public class SprBuilder : SpriteBuilder
    {
        private List<MemoryStream> _textureData = new();
        private List<SprSpriteEntry> _spriteEntries = new();
        private Dictionary<int, SprSpriteEntry> _newSpriteEntries = new();

        private SprHeader _sprHeader;
        private int _totalTextureSize= 0;
        public SprBuilder(Logger log) : base(log) { }

        public override void AddOrReplaceFile(string filePath)
        {
            var file = Path.GetFileName(filePath);

            switch (Path.GetExtension(file).ToLower())
            {
                case Constants.SprSpriteExtension:
                    _customSprFiles[filePath] = new(filePath);
                    break;
                case Constants.SprTextureExtension:
                    _customTextureFiles[filePath] = new(filePath);
                    break;
            }
        }

        public override MultiStream Build(nint handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0)
        {
            const int HEADER_LENGTH = 0x20;
            const int POINTER_ENTRY_LENGTH = 0x8;

            logger?.Info($"[{nameof(SprBuilder)}] Building SPR File | {{0}}", filepath);

            // Get original file's entries.
            _sprHeader = GetHeaderFromSpr<SprHeader>(handle, baseOffset);
            GetTextureDataFromSpr(handle, baseOffset);
            GetSpriteEntriesFromSpr(handle, baseOffset);

            // Write custom sprite entries from '.sprt' files to sprite dictionary
            foreach (var file in _customSprFiles.Values)
            {
                var stream = new FileStream(new SafeFileHandle(file.Handle, false), FileAccess.Read);

                string fileName = Path.GetFileNameWithoutExtension(file.FilePath);

                if (!fileName.StartsWith("spr_"))
                    continue;

                if (int.TryParse(fileName[4..], out int index))
                {
                    _newSpriteEntries[index] = stream.Read<SprSpriteEntry>();
                }

                stream.Dispose();
            }

            int nextId = _textureData.Count;

            // Get DDS filenames and adjust edited sprite texture ids
            foreach (var (key, file) in _customTextureFiles)
            {
                int newId = nextId;
                string fileName = Path.GetFileNameWithoutExtension(file.FilePath);

                if (fileName.StartsWith("spr_", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (int id in GetSpriteIdsFromFilename(fileName))
                    {
                        // Check for accompanying .spdspr file
                        _log.Info("boobs {0}", Path.GetDirectoryName(key) + $"\\spr_{id}{Constants.SprSpriteExtension}");
                        if (!_customSprFiles.ContainsKey(Path.GetDirectoryName(key) + $"\\spr_{id}{Constants.SprSpriteExtension}"))
                        {
                            if ( id < _spriteEntries.Count)
                            {
                                _newSpriteEntries[id] = _spriteEntries[id].ShallowCopy();
                            }
                        }

                        PatchSpriteEntry(id, newId);
                    }

                    nextId++;
                }
                else if (fileName.StartsWith("tex_", StringComparison.OrdinalIgnoreCase)) // Texture replacement is deprecated
                {
                    // Get texture id to replace from filename
                    if (!int.TryParse(fileName[4..].Split("_", StringSplitOptions.TrimEntries).FirstOrDefault(), out newId) || _textureData.Count == newId)
                        continue;

                    // Revert each modified sprite that used to point to the textures
                    for (int i = 0; i < _spriteEntries.Count; i++)
                    {
                        if (_spriteEntries[i].GetSpriteTextureId() == newId && _newSpriteEntries.ContainsKey(i))
                        {
                            _newSpriteEntries.Remove(i);
                        }
                    }
                }
                else continue;

                // Take texture data slice and make a stream out of it
                byte[] data = new byte[file.Length];
                file.GetData(data);

                if (newId >= _textureData.Count)
                    _textureData.Add(new MemoryStream(data));
                else
                    _textureData[newId] = new MemoryStream(data);
            }


            // Copy new sprite entries into the original sprite entry list
            foreach (var (index, value) in _newSpriteEntries)
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

            MemoryStream pointerStream = BuildPointerList();
            MemoryStream spriteStream = BuildSpriteStream();
            MemoryStream textureStream = BuildTextureDataStream();

            // Allocate Header
            MemoryStream headerStream = new(HEADER_LENGTH);

            // Write Header

            // Calculate filesize
            long newFileSize = HEADER_LENGTH + pointerStream.Length + spriteStream.Length + textureStream.Length;

            _sprHeader._fileSize = (int)newFileSize;
            _sprHeader._textureEntryCount= (short)_textureData.Count;
            _sprHeader._spriteEntryCount = (short)_spriteEntries.Count;
            _sprHeader._textureEntryOffset = HEADER_LENGTH;
            _sprHeader._spriteEntryOffset = HEADER_LENGTH + (_textureData.Count * POINTER_ENTRY_LENGTH);

            headerStream.Write(_sprHeader);

            // Calculate
            // Make Multistream
            var pairs = new List<StreamOffsetPair<Stream>>()
            {
                // Add Header
                new (headerStream, OffsetRange.FromStartAndLength(0, HEADER_LENGTH)),

                // Add Pointer Entries
                new (pointerStream, OffsetRange.FromStartAndLength(HEADER_LENGTH, pointerStream.Length)),

                // Add Sprites
                new (spriteStream, OffsetRange.FromStartAndLength(HEADER_LENGTH + pointerStream.Length, spriteStream.Length)),

                // Add Textures
                new (textureStream, OffsetRange.FromStartAndLength(HEADER_LENGTH + pointerStream.Length + spriteStream.Length, textureStream.Length))
            };

            return new MultiStream(pairs, logger);
        }

        /// <summary>
        /// Writes SPR pointer list to a stream.
        /// </summary>
        private MemoryStream BuildPointerList()
        {
            // Constants
            const int HEADER_SIZE = 0x20;
            const int POINTER_ENTRY_SIZE = 0x8;
            const int SPRITE_ENTRY_SIZE = 0x80;

            // Calculate pointer list sizes
            int pointerEntryListSize = (_spriteEntries.Count + _textureData.Count) * POINTER_ENTRY_SIZE;
            int spriteEntryListSize = SPRITE_ENTRY_SIZE * _spriteEntries.Count;

            MemoryStream stream = new(pointerEntryListSize);

            // Calculate the starting offsets of the sprite and texture listss
            int spriteEntryOffset = HEADER_SIZE + pointerEntryListSize;
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
                spriteEntryOffset += SPRITE_ENTRY_SIZE;
            }

            return stream;
        }

        /// <summary>
        /// Writes SPR sprite entries to a stream.
        /// </summary>
        private MemoryStream BuildSpriteStream()
        {
            const int SPRITE_ENTRY_SIZE = 0x80;

            MemoryStream stream = new(_spriteEntries.Count * SPRITE_ENTRY_SIZE);

            foreach (var sprite in _spriteEntries)
            {
                stream.Write(sprite);
            }

            return stream;
        }

        /// <summary>
        /// Writes raw textures to a stream.
        /// </summary>
        private MemoryStream BuildTextureDataStream()
        {
            // data stream
            MemoryStream stream = new(_totalTextureSize);

            // Write original textures
            foreach (var texture in _textureData)
            {
                texture.WriteTo(stream);
            }

            return stream;
        }

        private void PatchSpriteEntry(int spriteId, int newTextureId)
        {
            if (!_newSpriteEntries.ContainsKey(spriteId))
            {
                _log.Info("Tried to patch non-existent SPR id {0}. Skipping...", spriteId);
                return;
            }

            var spriteEntry = _newSpriteEntries[spriteId];
            spriteEntry.SetTextureId(newTextureId);
            _newSpriteEntries[spriteId] = spriteEntry;
        }

        private void GetTextureDataFromSpr(IntPtr handle, long pos)
        {
            var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

            try
            {
                var (count, offset) = _sprHeader.GetTextureEntryCountAndOffset();
                stream.Seek(offset, SeekOrigin.Begin);

                for (int i = 0; i < count; i++)
                {
                    var pointer = stream.Read<SprPointer>();
                    _textureData.Add(ReadTmx(stream, pointer.GetOffset()));
                }
            }
            finally
            {
                stream.Dispose();
                _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
            }
        }

        private void GetSpriteEntriesFromSpr(IntPtr handle, long pos)
        {
            var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

            try
            {
                var (count, offset) = _sprHeader.GetSpriteEntryCountAndOffset();
                stream.Seek(offset, SeekOrigin.Begin);

                for (int i = 0; i < count; i++)
                {
                    var pointer = stream.Read<SprPointer>();
                    _spriteEntries.Add(ReadSprite(stream, pointer.GetOffset()));
                }
            }
            finally
            {
                stream.Dispose();
                _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
            }
        }

        private MemoryStream ReadTmx(Stream stream, long tmxOffset)
        {
            // Hold current stream position
            long pos = stream.Position;

            // Seek tmx offset in spr
            stream.Seek(tmxOffset, SeekOrigin.Begin);

            // Read tmx size from tmx
            stream.Read<int>();
            var tmxSize = stream.Read<int>();

            // Go back to the beginning of the tmx
            stream.Seek(tmxOffset, SeekOrigin.Begin);
            
            // Read tmx bytes into the buffer
            var tmxBytes = new byte[tmxSize];
            stream.Read(tmxBytes);

            // Return stream to the original position
            stream.Seek(pos, SeekOrigin.Begin);

            return new MemoryStream(tmxBytes);
        }

        private SprSpriteEntry ReadSprite(Stream stream, long spriteOffset)
        {
            long pos = stream.Position;

            stream.Seek(spriteOffset, SeekOrigin.Begin);
            var sprite = stream.Read<SprSpriteEntry>();
            stream.Seek(pos, SeekOrigin.Begin);

            return sprite;
        }
    }
}
