using System.IO;
using AWB.Stream.Emulator.Acb;
using AWB.Stream.Emulator.Awb;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.Sigscan;
using Xunit;

namespace FileEmulationFramework.Tests.Emulators.AWB;

/// <summary>
/// Basic tests for patching ACBs.
/// </summary>
public unsafe class AcbPatcherTests
{
    [Fact]
    public void Baseline()
    {
        // Read the original data.
        using var originalStream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        var originalData = new byte[originalStream.Length];
        originalStream.TryRead(originalData, out _);
        originalStream.Position = 0;

        // Setup the necessary data for injection.
        var entry   = AcbPatcherEntry.FromAwbStream(originalStream, Assets.AwbEmulatorSampleFile);
        var factory = new ScannerFactory();
        Assert.True(AwbHeaderReader.TryHashHeader(originalStream, out ulong hash));
        originalStream.Position = 0;
        
        // Try inject.
        fixed (byte* dataPtr = &originalData[0])
        {
            Assert.True(AcbPatcher.TryInjectAwbHeader(factory, dataPtr, originalData.Length, hash, entry));
        }
    }
    
    [Fact]
    public void BadHash_Fails()
    {
        // Read the original data.
        using var originalStream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        var originalData = new byte[originalStream.Length];
        originalStream.TryRead(originalData, out _);
        originalStream.Position = 0;

        // Setup the necessary data for injection.
        var entry   = AcbPatcherEntry.FromAwbStream(originalStream, Assets.AwbEmulatorSampleFile);
        var factory = new ScannerFactory();
        
        Assert.True(AwbHeaderReader.TryHashHeader(originalStream, out ulong hash));
        originalStream.Position = 0;
        
        // Try inject.
        fixed (byte* dataPtr = &originalData[0])
        {
            // Oops our hash is borked!
            Assert.False(AcbPatcher.TryInjectAwbHeader(factory, dataPtr, originalData.Length, hash + 1, entry));
        }
    }
    
    [Fact]
    public void NoAfs2Header_Fails()
    {
        // Read the original data.
        using var originalStream = new FileStream(Assets.AwbEmulatorSampleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        var originalData = new byte[originalStream.Length];
        originalStream.TryRead(originalData, out _);
        originalStream.Position = 0;

        // Setup the necessary data for injection.
        var entry   = AcbPatcherEntry.FromAwbStream(originalStream, Assets.AwbEmulatorSampleFile);
        var factory = new ScannerFactory();
        
        Assert.True(AwbHeaderReader.TryHashHeader(originalStream, out ulong hash));
        originalStream.Position = 0;
        
        // Try inject.
        fixed (byte* dataPtr = &originalData[0])
        {
            // Oops our hash is borked!
            *dataPtr = 69; // break the Magic
            Assert.False(AcbPatcher.TryInjectAwbHeader(factory, dataPtr, originalData.Length, hash, entry));
        }
    }
}