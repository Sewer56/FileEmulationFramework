namespace SPD.File.Emulator.Spr;

public struct SprPointer
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private membersu16 flags;
    int unk00;
    int offset;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
    public SprPointer(int offset)
    {
        unk00 = 0;
        this.offset = offset;
    }
    public int GetOffset() => offset;
}