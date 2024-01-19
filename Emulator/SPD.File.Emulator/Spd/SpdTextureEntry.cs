namespace SPD.File.Emulator.Spd;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
// ReSharper disable InconsistentNaming

public struct SpdTextureEntry
{
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
}