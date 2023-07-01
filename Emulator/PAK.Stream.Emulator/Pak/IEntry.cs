namespace PAK.Stream.Emulator.Pak;

public interface IEntry : IDisposable
{
    string FileName { get; }

    int Length { get; }
}