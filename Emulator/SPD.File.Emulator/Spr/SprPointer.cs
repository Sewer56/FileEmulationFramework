namespace SPD.File.Emulator.Spr;

public struct SprPointer
{
#pragma warning disable IDE0044 // Add readonly modifier
    int unk00;
    int offset;
#pragma warning restore IDE0044 // Add readonly modifier
    public SprPointer(int offset)
    {
        unk00 = 0;
        this.offset = offset;
    }
    public int GetOffset() => offset;
}