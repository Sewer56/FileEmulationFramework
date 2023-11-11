using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SPD.File.Emulator.Spd;

public struct SpdTextureEntry
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    int _textureId;
    int _unk04;
    int _textureDataOffset;
    int _textureDataSize;
    int _textureWidth;
    int _textureHeight;
    int _unk18;
    int _unk1c;
    unsafe fixed byte _textureName[16];

    public readonly int GetTextureId() => _textureId;
    public readonly (int, int) GetTextureOffsetAndSize() => (_textureDataOffset, _textureDataSize);
    public void SetTextureOffset(int newOffset) => _textureDataOffset = newOffset;
    public void SetTextureId(int id) => _textureId = id;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}