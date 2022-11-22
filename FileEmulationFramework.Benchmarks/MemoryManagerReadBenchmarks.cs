using BenchmarkDotNet.Attributes;
using FileEmulationFramework.Lib.Memory;

namespace FileEmulationFramework.Benchmarks;

[MemoryDiagnoser]
public class MemoryManagerReadBenchmarks
{
    // 32 MiB
    private byte[] _dataToWrite = new byte[1024 * 1024 * 32];

    private MemoryStream _memoryStream = null!;
    private MemoryManagerStream _memoryManagerStream64K = null!;
    private MemoryManagerStream _memoryManagerStream4M = null!;

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
        _memoryManagerStream64K = new MemoryManagerStream(new MemoryManager(65536), true);
        _memoryManagerStream64K.Write(_dataToWrite);

        _memoryManagerStream4M = new MemoryManagerStream(new MemoryManager(65536 * 64), true);
        _memoryManagerStream4M.Write(_dataToWrite);
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
        _memoryManagerStream64K.Position = 0;
        _memoryManagerStream64K.ReadExactly(_dataToWrite);
    }

    [Benchmark]
    public void MemoryManagerStream_4M()
    {
        _memoryManagerStream4M.Position = 0;
        _memoryManagerStream4M.ReadExactly(_dataToWrite);
    }
}