namespace SPD.File.Emulator.Spd;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
// ReSharper disable InconsistentNaming

public struct SpdSpriteEntry
{
    int _spriteId;
    int _spriteTextureId;
    int _unk08;
    int _unk0c;
    int _unk10;
    int _unk14;
    int _unk18;
    int _unk1c;
    int _spriteXPosition;
    int _spriteYPosition;
    int _spriteXLength;
    int _spriteYLength;
    int _unk30;
    int _unk34;
    int _spriteXScale;
    int _spriteYScale;
    int _unk40;
    int _unk44;
    int _unk48;
    int _unk4c;
    int _unk50;
    int _unk54;
    int _unk58;
    int _unk5c;
    int _unk60;
    int _unk64;
    int _unk68;
    int _unk6c;
    unsafe fixed byte _spriteName[48];

    public readonly int GetSpriteId() => _spriteId;
    public readonly int GetSpriteTextureId() => _spriteTextureId;
    public void SetTextureId(int id) => _spriteTextureId = id;
}