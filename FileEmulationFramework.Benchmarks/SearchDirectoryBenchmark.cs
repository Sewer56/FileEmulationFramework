using BenchmarkDotNet.Attributes;
using FileEmulationFramework.Lib.IO;

namespace FileEmulationFramework.Benchmarks;

[MemoryDiagnoser]
public class SearchDirectoryBenchmark
{
    /// <summary>
    /// Path to the search folder.
    /// </summary>
    public static string SearchPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86))!.FullName;

    [Benchmark]
    public List<FileInformation> Current()
    {
        WindowsDirectorySearcher.GetDirectoryContentsRecursive(SearchPath, out var result, out _);
        return result;
    }

    [Benchmark]
    public List<FileInformation> CurrentMt()
    {
        WindowsDirectorySearcher.GetDirectoryContentsRecursive(SearchPath, out var result, out _, true);
        return result;
    }

    [Benchmark]
    public List<DirectoryFilesGroup> CurrentGroup()
    {
        WindowsDirectorySearcher.GetDirectoryContentsRecursiveGrouped(SearchPath, out var result);
        return result;
    }
    
    [Benchmark]
    public List<DirectoryFilesGroup> CurrentGroupMt()
    {
        WindowsDirectorySearcher.GetDirectoryContentsRecursiveGrouped(SearchPath, out var result, true);
        return result;
    }
    
    [Benchmark]
    public List<Legacy.FileInformation> Legacy()
    {
        Benchmarks.Legacy.WindowsDirectorySearcher.GetDirectoryContentsRecursive(SearchPath, out var result, out _);
        return result;
    }
}