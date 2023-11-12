namespace SPD.File.Emulator.Spd;

public struct SpdHeader
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    int magic;
    int _unk04;
    internal int fileSize;
    int _unk0c;
    int _unk10;
    internal short textureEntryCount;
    internal short spriteEntryCount;
    internal int textureEntryOffset;
    internal int spriteEntryOffset;

    public readonly (short, int) GetTextureEntryCountAndOffset() => (textureEntryCount, textureEntryOffset);
    public readonly (short, int) GetSpriteEntryCountAndOffset() => (spriteEntryCount, spriteEntryOffset);
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}