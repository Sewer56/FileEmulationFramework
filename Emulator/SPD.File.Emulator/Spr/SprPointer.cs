namespace SPD.File.Emulator.Spr;

#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable IDE0044 // Add readonly modifier
// ReSharper disable InconsistentNaming

public struct SprPointer
{
    int unk00;
    int offset;

    public SprPointer(int offset)
    {
        unk00 = 0;
        this.offset = offset;
    }

    public int GetOffset() => offset;
}