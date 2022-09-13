using BenchmarkDotNet.Attributes;
using FileEmulationFramework.Lib;

namespace FileEmulationFramework.Benchmarks;

/// <summary>
/// Note: Doesn't run on
/// </summary>
[MemoryDiagnoser]
public class RouteMergingBenchmarks
{
    private string _currentRoute = null!;
    private string _newFilePath = null!;
    private string _newFilePathLong = null!;

    [GlobalSetup]
    public void Setup()
    {
        _currentRoute = NormalizeSlashes("Data/BGM/VOICE.AFS");
        _newFilePath = NormalizeSlashes("FEmulator/AFS/VOICE.AFS/00000.adx");
        _newFilePathLong = NormalizeSlashes("FEmulator/AFS/VOICE.AFS/NestedFolderA/NestedFolderB/00000.adx");
    }

    [Benchmark]
    public Route Merge_WithShortPath()
    {
        // Arrange
        var route = new Route(_currentRoute);

        for (int x = 0; x < 1000; x++)
            route.Merge(_newFilePath);

        return route;
    }

    [Benchmark]
    public Route Merge_WithLongPaths()
    {
        // Arrange
        var route = new Route(_currentRoute);

        for (int x = 0; x < 1000; x++)
            route.Merge(_newFilePathLong);

        return route;
    }

    private string NormalizeSlashes(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }
}