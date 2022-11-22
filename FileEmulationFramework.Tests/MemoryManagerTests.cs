using System;
using FileEmulationFramework.Lib.Memory;
using Xunit;

namespace FileEmulationFramework.Tests;

public class MemoryManagerTests
{
    [Fact]
    public void MemoryManager_CanAllocate_WithoutThrowing()
    {
        using var manager = new MemoryManager(65536);
        manager.Allocate(4096);
    }

    [Fact]
    public void MemoryManager_GetMappedFile_ThrowsWithoutSufficientData()
    {
        using var manager = new MemoryManager(65536);
        Assert.Throws<ArgumentOutOfRangeException>(() => manager.GetMappedFile(4096, out _, out _));
    }

    [Fact]
    public void MemoryManager_GetMappedFile_Baseline()
    {
        using var manager = new MemoryManager(65536);
        manager.Allocate(4096);
        manager.GetMappedFile(4096, out int offset, out int available);
        Assert.Equal(4096, offset);
        Assert.Equal(61440, available);
    }

    [Fact]
    public void MemoryManager_GetMappedFileWithAlloc_AllocatesSingle()
    {
        using var manager = new MemoryManager(65536);
        manager.GetMappedFileWithAlloc(4096, out _, out _);
        Assert.Equal(1, manager.PageCount);
    }

    [Fact]
    public void MemoryManager_GetMappedFileWithAlloc_AllocatesDouble()
    {
        using var manager = new MemoryManager(65536);
        manager.GetMappedFileWithAlloc((65536 * 2) - 1, out int offset, out int available);
        Assert.Equal(2, manager.PageCount);
        Assert.Equal(65535, offset);
        Assert.Equal(1, available);
    }

    [Fact]
    public void MemoryManager_GetMappedFileWithAlloc_AllocatesTriple_OnPageBound()
    {
        using var manager = new MemoryManager(65536);
        manager.GetMappedFileWithAlloc((65536 * 2), out int offset, out int available);
        Assert.Equal(3, manager.PageCount);
        Assert.Equal(0, offset);
        Assert.Equal(65536, available);
    }
}