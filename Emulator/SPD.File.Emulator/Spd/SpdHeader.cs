namespace SPD.File.Emulator.Spd;

public struct SpdHeader
{
    unsafe fixed char _magic[4];
    int _unk04;
    int _fileSize;
    int _unk0c;
    int _unk10;
    short _textureEntryCount;
    short _spriteEntryCount;
    int _textureEntryOffset;
    int _spriteEntryOffset;
}