using System.IO;
using System.Linq;
using FileEmulationFramework.Lib.IO;
using Xunit;

namespace FileEmulationFramework.Tests;

/// <summary>
/// Baseline tests for the Windows Directory Searcher.
/// </summary>
public class SearchTests
{
    [Fact]
    public void Search_NonGrouped_FileCountMatches()
    {
        var systemIo = Directory.GetFiles(Assets.EmulatorsDirectory, "*.*", SearchOption.AllDirectories);
        WindowsDirectorySearcher.GetDirectoryContentsRecursive(Assets.EmulatorsDirectory, out var files, out _);
        Assert.Equal(systemIo.Length, files.Count);
    }
    
    [Fact]
    public void Search_Grouped_FileCountMatches()
    {
        var systemIo = Directory.GetFiles(Assets.EmulatorsDirectory, "*.*", SearchOption.AllDirectories);
        WindowsDirectorySearcher.GetDirectoryContentsRecursiveGrouped(Assets.EmulatorsDirectory, out var groups);
        Assert.Equal(systemIo.Length, groups.Sum(x => x.Files.Length));
    }
    
    [Fact]
    public void Search_NonGrouped_FileCountMatches_MultiThreaded()
    {
        var systemIo = Directory.GetFiles(Assets.EmulatorsDirectory, "*.*", SearchOption.AllDirectories);
        WindowsDirectorySearcher.GetDirectoryContentsRecursive(Assets.EmulatorsDirectory, out var files, out _, true);
        Assert.Equal(systemIo.Length, files.Count);
    }
    
    [Fact]
    public void Search_Grouped_FileCountMatches_MultiThreaded()
    {
        var systemIo = Directory.GetFiles(Assets.EmulatorsDirectory, "*.*", SearchOption.AllDirectories);
        WindowsDirectorySearcher.GetDirectoryContentsRecursiveGrouped(Assets.EmulatorsDirectory, out var groups, true);
        Assert.Equal(systemIo.Length, groups.Sum(x => x.Files.Length));
    }
}