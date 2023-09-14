namespace SPD.File.Emulator.Spd;

public struct SpdHeader
{
    int magic;
    int _unk04;
    int _fileSize;
    int _unk0c;
    int _unk10;
    short _textureEntryCount;
    short _spriteEntryCount;
    int _textureEntryOffset;
    int _spriteEntryOffset;

    public (short, int) GetTextureEntryCountAndOffset() => (_textureEntryCount, _textureEntryOffset);
    public (short, int) GetSpriteEntryCountAndOffset() => (_spriteEntryCount, _spriteEntryOffset);
}