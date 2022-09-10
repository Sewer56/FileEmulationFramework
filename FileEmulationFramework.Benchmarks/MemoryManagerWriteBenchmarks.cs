using BenchmarkDotNet.Attributes;
using FileEmulationFramework.Lib.Memory;

namespace FileEmulationFramework.Benchmarks;

[MemoryDiagnoser]
public class MemoryManagerWriteBenchmarks
{
    // 32 MiB
    private byte[] _dataToWrite = new byte[1024 * 1024 * 32];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random();
        for (int x = 0; x < _dataToWrite.Length; x++)
            _dataToWrite[x] = (byte)random.Next();
    }

    [Benchmark]
    public void MemoryStream()
    {
        using var memoryStream = new MemoryStream();
        memoryStream.Write(_dataToWrite);
    }
    
    [Benchmark]
    public void MemoryManagerStream_64K()
    {
        using var memoryManager = new MemoryManager(65536);
        using var memoryManagerStream = new MemoryManagerStream(memoryManager, true);
        memoryManagerStream.Write(_dataToWrite);
    }

    [Benchmark]
    public void MemoryManagerStream_4M()
    {
        using var memoryManager = new MemoryManager(1024 * 1024 * 4);
        using var memoryManagerStream = new MemoryManagerStream(memoryManager, true);
        memoryManagerStream.Write(_dataToWrite);
    }
}