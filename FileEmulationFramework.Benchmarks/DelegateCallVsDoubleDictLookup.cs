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

    private Dictionary<nint, ISwag> _dictToInterface;
    private Dictionary<nint, ISwag> _dictToInterface2;
    private Dictionary<nint, Action<IntPtr, IntPtr>> _dictToFunctionPointer;

    [Params(2, 4, 8, 16, 32, 64, 128)] public int N;

    [GlobalSetup]
    public void Setup()
    {
        _dictToInterface = new Dictionary<nint, ISwag>(N);
        _dictToInterface2 = new Dictionary<nint, ISwag>(N);
        _dictToFunctionPointer = new Dictionary<nint, Action<IntPtr, IntPtr>>(N);

        for (int i = 0; i < N; i++)
        {
            _dictToInterface[i] = new Swag();
            _dictToInterface2[i] = new Swag();
            _dictToFunctionPointer[i] = _staticSwag.Invoke;
        }
    }

    [Benchmark]
    public void SeparateDicts()
    {
        for (int x = 0; x < N; x++)
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
        for (int x = 0; x < N; x++)
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
}

public class Swag : ISwag
{
    public void Invoke(IntPtr a, IntPtr b)
    {
    }
}

public interface ISwag
{
    public void Invoke(IntPtr a, IntPtr b);
}