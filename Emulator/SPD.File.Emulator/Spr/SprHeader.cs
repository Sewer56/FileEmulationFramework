namespace SPD.File.Emulator.Spr;

public struct SprHeader
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private membersu16 flags;
    short _flags;
    short _userId;
    int _reserved1;
    int magic;      // 'SPR0'
    int _headerSize;        // always 2, maybe version
    internal int _fileSize;
    internal short _textureEntryCount;
    internal short _spriteEntryCount;
    internal int _textureEntryOffset;
    internal int _spriteEntryOffset;

    public readonly (short, int) GetTextureEntryCountAndOffset() => (_textureEntryCount, _textureEntryOffset);
    public readonly (short, int) GetSpriteEntryCountAndOffset() => (_spriteEntryCount, _spriteEntryOffset);
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}