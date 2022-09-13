using BenchmarkDotNet.Attributes;
using FileEmulationFramework.Lib.Memory;

namespace FileEmulationFramework.Benchmarks;

[MemoryDiagnoser]
public class MemoryManagerReadBenchmarks
{
    // 32 MiB
    private byte[] _dataToWrite = new byte[1024 * 1024 * 32];

    private MemoryStream _memoryStream = null!;
    private MemoryManagerStream _memoryManagerStream_64k = null!;
    private MemoryManagerStream _memoryManagerStream_4M = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random();
        for (int x = 0; x < _dataToWrite.Length; x++)
            _dataToWrite[x] = (byte)random.Next();

        // Init MemoryStream
        _memoryStream = new MemoryStream();
        _memoryStream.Write(_dataToWrite);

        // Init MemoryManagerStream
        _memoryManagerStream_64k = new MemoryManagerStream(new MemoryManager(65536), true);
        _memoryManagerStream_64k.Write(_dataToWrite);

        _memoryManagerStream_4M = new MemoryManagerStream(new MemoryManager(65536 * 64), true);
        _memoryManagerStream_4M.Write(_dataToWrite);
    }

    [Benchmark]
    public void MemoryStream()
    {
        _memoryStream.Position = 0;
        _memoryStream.ReadExactly(_dataToWrite);
    }

    [Benchmark]
    public void MemoryManagerStream_64K()
    {
        _memoryManagerStream_64k.Position = 0;
        _memoryManagerStream_64k.ReadExactly(_dataToWrite);
    }

    [Benchmark]
    public void MemoryManagerStream_4M()
    {
        _memoryManagerStream_4M.Position = 0;
        _memoryManagerStream_4M.ReadExactly(_dataToWrite);
    }
}