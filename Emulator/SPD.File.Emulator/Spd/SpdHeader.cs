namespace SPD.File.Emulator.Spd;

public struct SpdHeader
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    int magic;
    int _unk04;
    int _fileSize;
    int _unk0c;
    int _unk10;
    short _textureEntryCount;
    short _spriteEntryCount;
    int _textureEntryOffset;
    int _spriteEntryOffset;

    public readonly (short, int) GetTextureEntryCountAndOffset() => (_textureEntryCount, _textureEntryOffset);
    public readonly (short, int) GetSpriteEntryCountAndOffset() => (_spriteEntryCount, _spriteEntryOffset);
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}