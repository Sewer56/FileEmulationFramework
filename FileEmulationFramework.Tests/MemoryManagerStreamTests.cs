using System;
using System.IO;
using FileEmulationFramework.Lib.Memory;
using FileEmulationFramework.Tests.Extensions;
using Reloaded.Memory.Streams;
using Xunit;

namespace FileEmulationFramework.Tests;

public class MemoryManagerStreamTests
{
    [Fact]
    public void Can_Read_Write_Seek_Current_InSameBuffer()
    {
        Can_Read_Write_Seek_InSameBuffer_Base(managerStream => managerStream.Seek(-sizeof(int), SeekOrigin.Current));
    }

    [Fact]
    public void Can_Read_Write_Seek_Begin_InSameBuffer()
    {
        Can_Read_Write_Seek_InSameBuffer_Base(managerStream => managerStream.Seek(0, SeekOrigin.Begin));
    }

    [Fact]
    public void Can_Read_Write_Seek_End_InSameBuffer()
    {
        Can_Read_Write_Seek_InSameBuffer_Base(managerStream => managerStream.Seek(-sizeof(int), SeekOrigin.End));
    }

    [Fact]
    public void Can_Read_Write_Seek_Current_InMultipleBuffers()
    {
        Can_Read_Write_Seek_InMultipleBuffers_Base(managerStream => managerStream.Seek(-sizeof(int), SeekOrigin.Current));
    }

    [Fact]
    public void Can_Read_Write_Seek_Begin_InMultipleBuffers()
    {
        Can_Read_Write_Seek_InMultipleBuffers_Base(managerStream => managerStream.Seek(managerStream.Position - sizeof(int), SeekOrigin.Begin));
    }

    [Fact]
    public void Can_Read_Write_Seek_End_InMultipleBuffers()
    {
        Can_Read_Write_Seek_InMultipleBuffers_Base(managerStream => managerStream.Seek(-sizeof(int), SeekOrigin.End));
    }

    private void Can_Read_Write_Seek_InSameBuffer_Base(Action<MemoryManagerStream> seekFn)
    {
        const int expected = 0x11224488;
        using var manager = new MemoryManager(65536);
        using var stream = new MemoryManagerStream(manager, true);

        stream.Write(expected);
        seekFn(stream);
        var actual = stream.Read<int>();

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(sizeof(int), stream.Length);
        Assert.Equal(sizeof(int), stream.Position);
    }

    private void Can_Read_Write_Seek_InMultipleBuffers_Base(Action<MemoryManagerStream> seekFn)
    {
        const int expected = 0x11224488;
        const int allocationGranularity = 65536;
        const int halfDataSize = sizeof(int) / 2;
        using var manager = new MemoryManager(allocationGranularity);
        using var stream  = new MemoryManagerStream(manager, true);

        // Seek, so our value will be written between 2 buffers.
        stream.Seek(allocationGranularity - halfDataSize, SeekOrigin.Begin);

        // Write the value, split between the buffers.
        stream.Write(expected);
        seekFn(stream);
        var actual = stream.Read<int>();

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(allocationGranularity + halfDataSize, stream.Length);
        Assert.Equal(allocationGranularity + halfDataSize, stream.Position);
    }
}