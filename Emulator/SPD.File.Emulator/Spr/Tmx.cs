using FileEmulationFramework.Lib.Utilities;

namespace SPD.File.Emulator.Spr;

public struct Tmx
{
#pragma warning disable IDE0044 // Add readonly modifier
    int _unk00;
    int _fileSize;
    byte[] _data;
#pragma warning restore IDE0044 // Add readonly modifier
    public Tmx()
    {
        _unk00 = 0;
        _fileSize = 0;
        _data = Array.Empty<byte>();
    }
    public Tmx(Stream stream)
    {
        _unk00 = stream.Read<int>();
        _fileSize = stream.Read<int>();
        _data = new byte[_fileSize - 8];

        stream.TryRead(_data, out _);
    }
    public void Initialize(Stream stream)
    {
        _unk00 = stream.Read<int>();
        _fileSize = stream.Read<int>();
        _data = new byte[_fileSize - 8];

        stream.TryRead(_data, out _);
    }
    public int GetFilesize() => _fileSize;
}