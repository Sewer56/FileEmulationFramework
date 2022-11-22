using BenchmarkDotNet.Attributes;

namespace FileEmulationFramework.Benchmarks;

/// <summary>
/// A set of benchmarks for testing single lookup + delegate call vs double lookup and interface call.
/// This benchmark will influence how RegisterCustomFile in IEmulationFramework will be implemented
/// potential change to how FileAccessServer will work.
/// </summary>
public class DelegateCallVsDoubleDictLookup
{
    private static Swag _staticSwag = new();
    
    // Simulates scenario with two lookups, separate for RegisterCustomFile and Emulated Files.
    private Dictionary<nint, ISwag> _dictToInterface = new();
    private Dictionary<nint, ISwag> _dictToInterface2 = new();
    
    private Dictionary<nint, Action<IntPtr, IntPtr>> _dictToFunctionPointer = new();
    
    [GlobalSetup]
    public void Setup()
    {
        _dictToInterface[0]       = new Swag();
        _dictToInterface2[0]      = new Swag(); // We can assume calling emu interface and custom registered file to have same overhead.
        _dictToFunctionPointer[0] = _staticSwag.Invoke; // Delegates are slower, but one lookup might be faster
    }

    // In these benchmarks we simulate a 1/10 hit rate to give an estimate of emulated to non-emulated files.
    
    [Benchmark]
    public void SeparateDicts()
    {
        for (int x = 0; x < 10; x++)
        {
            if (_dictToInterface.TryGetValue(x, out var iFace))
                iFace.Invoke(IntPtr.Zero, IntPtr.Zero);

            if (_dictToInterface2.TryGetValue(x, out iFace))
                iFace.Invoke(IntPtr.Zero, IntPtr.Zero);
        }
    }
    
    [Benchmark]
    public void UnifiedLookup()
    {
        for (int x = 0; x < 10; x++)
        {
            if (_dictToFunctionPointer.TryGetValue(x, out var iFace))
                iFace(IntPtr.Zero, IntPtr.Zero);
        }
    }
    
    [Benchmark]
    public void SingleInterfaceCall()
    {
        if (_dictToInterface.TryGetValue(0, out var iFace))
            iFace.Invoke(IntPtr.Zero, IntPtr.Zero);
    }
    
    [Benchmark]
    public void SingleDelegateCall()
    {
        if (_dictToInterface.TryGetValue(0, out var iFace))
            iFace.Invoke(IntPtr.Zero, IntPtr.Zero);
    }
}

public class Swag : ISwag
{
    public void Invoke(IntPtr a, IntPtr b) { }
}

public interface ISwag
{
    public void Invoke(IntPtr a, IntPtr b);
}