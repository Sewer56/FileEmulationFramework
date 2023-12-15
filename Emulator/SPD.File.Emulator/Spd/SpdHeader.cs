
using System.Diagnostics.CodeAnalysis;

namespace SPD.File.Emulator.Spd;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public struct SpdHeader
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // Field is never used
    int _magic;
    int _unk04;
    internal int FileSize;
    int _unk0c;
    int _unk10;
    internal short TextureEntryCount;
    internal short SpriteEntryCount;
    internal int TextureEntryOffset;
    internal int SpriteEntryOffset;

    public readonly (short, int) GetTextureEntryCountAndOffset() => (TextureEntryCount, TextureEntryOffset);
    public readonly (short, int) GetSpriteEntryCountAndOffset() => (SpriteEntryCount, SpriteEntryOffset);
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CS0169 // Field is never use
}