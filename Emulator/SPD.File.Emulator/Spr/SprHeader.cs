namespace SPD.File.Emulator.Spr;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private membersu16 flags;
#pragma warning disable CS0169 // Field is never used
// ReSharper disable InconsistentNaming

public struct SprHeader
{
    short _flags;
    short _userId;
    int _reserved1;
    int magic;      // 'SPR0'
    int _headerSize;        // always 2, maybe version
    internal int FileSize;
    internal short TextureEntryCount;
    internal short SpriteEntryCount;
    internal int TextureEntryOffset;
    internal int SpriteEntryOffset;

    public readonly (short, int) GetTextureEntryCountAndOffset() => (TextureEntryCount, TextureEntryOffset);
    public readonly (short, int) GetSpriteEntryCountAndOffset() => (SpriteEntryCount, SpriteEntryOffset);
}