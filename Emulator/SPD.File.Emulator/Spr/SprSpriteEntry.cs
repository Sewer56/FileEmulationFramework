namespace SPD.File.Emulator.Spr;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // Field is never used
// ReSharper disable InconsistentNaming

public struct SprSpriteEntry
{
    int _unk0x00;
    unsafe fixed byte _comment[16];
    int _textureIndex;
    int _unk0x18;   // some sort of id?
    int _unk0x1C;
    int _unk0x20;
    int _unk0x24;
    int _unk0x28;
    int _unk0x2C;
    int _unk0x30;
    int _unk0x34;
    int _unk0x38;
    int _unk0x3C;
    int _unk0x40;   // set in 'center' frames?
    int _mOffsetX;
    int _mOffsetY;
    int _unk0x4C;
    int _unk0x50;
    int _mCoordX1;
    int _mCoordY1;
    int _mCoordX2;
    int _mCoordY2;
    int _unk0x64;   // argb color
    int _unk0x68;   // argb color
    int _unk0x6C;   // argb color
    int _unk0x70;   // argb color
    int _unk0x74;   // possibly padding
    int _unk0x78;   // possibly padding
    int _unk0x7C;   // possibly padding

    public readonly int GetSpriteTextureId() => _textureIndex;
    public void SetTextureId(int id)
    {
        _textureIndex = id;
    }
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}