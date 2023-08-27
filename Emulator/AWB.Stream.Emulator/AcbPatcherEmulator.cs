using AWB.Stream.Emulator.Acb;
using AWB.Stream.Emulator.Acb.Utilities;
using AWB.Stream.Emulator.Awb;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Interfaces.Reference;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Sigscan.Definitions;

namespace AWB.Stream.Emulator;

/// <summary>
/// Emulator that patches.
/// </summary>
public class AcbPatcherEmulator : IEmulator
{
    /// <summary>
    /// If enabled, dumps newly emulated files.
    /// </summary>
    public bool DumpFiles { get; set; }
    
    private readonly Logger _log;
    private readonly IScannerFactory _scannerFac;
    
    private readonly Dictionary<ulong, AcbPatcherEntry> _headerHashToHeader = new();
    private readonly Dictionary<string, MemoryStream?> _pathToStream = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AwbToAcbEntry?> _awbToAcbMap = new(StringComparer.OrdinalIgnoreCase);
    private bool _checkAcbExtension;

    public AcbPatcherEmulator(AwbEmulator awbEmulator, Logger log, IScannerFactory scannerFactory, bool checkAcbExtension, bool dumpFiles)
    {
        _scannerFac = scannerFactory;
        _log = log;
        _checkAcbExtension = checkAcbExtension;
        DumpFiles = dumpFiles;
        awbEmulator.OnStreamCreated += OnAwbCreated;
    }

    private void OnAwbCreated(IntPtr handle, string awbPath, MultiStream newAwbStream)
    {
        if (!AwbHeaderReader.TryHashHeader(handle, out var hash))
        {
            _log.Warning("[AcbPatcherEmulator] Couldn't hash header of newly created AWB file... what?");
            return;
        }
        
        _headerHashToHeader[hash] = AcbPatcherEntry.FromAwbStream(newAwbStream, awbPath);
        _log.Info("[AcbPatcherEmulator] Added AWB Header Hash {0} for File {1}", hash, awbPath);
    }

    public unsafe bool TryCreateFile(IntPtr handle, string filepath, string route, out IEmulatedFile emulatedFile)
    {
        // Check if we already made a custom ACB for this file.
        emulatedFile = null!;
        if (_pathToStream.TryGetValue(filepath, out var multiStream))
        {
            // Avoid recursion into same file.
            if (multiStream == null)
                return false;

            emulatedFile = new EmulatedFile<MemoryStream>(multiStream);
            return true;
        }

        // Check for extension as speedup
        var extension = Path.GetExtension(filepath.AsSpan());
        if (_checkAcbExtension && !extension.Equals(".acb", StringComparison.OrdinalIgnoreCase) && // standard
            !extension.Equals(".bdx", StringComparison.OrdinalIgnoreCase)) // bayonetta
            return false;
        
        // Check file type.
        if (!AcbChecker.IsAcbFile(handle))
            return false;

        // Make the new file.
        _pathToStream[filepath] = null; // Avoid recursion into same file.
        
        var fileStream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
        var pos = fileStream.Position;
        MemoryStream stream;
        try
        {
            // Read the data for file.
            var data = GC.AllocateUninitializedArray<byte>((int)fileStream.Length);
            fileStream.ReadExactly(data);
            
            // Try inject any known AWB
            fixed (byte* dataPtr = &data[0])
            {
                if (!AcbPatcher.TryHashAwbHeader(_scannerFac, dataPtr, data.Length, out var afs2HeaderPtr, out var hash))
                {
                    _pathToStream.Remove(filepath);
                    return false;
                }

                _log.Info("[AcbPatcherEmulator] ACB Hash {0} for {1}.", hash, filepath);
                if (!_headerHashToHeader.TryGetValue(hash, out var patcherEntry))
                {
                    _log.Info("[AcbPatcherEmulator] No AWB entry for ACB found {0}, gonna try opening file.", filepath);

                    // No entry to patch, some games can open ACB before AWB, so let's try open AWB if it exists.
                    var awbPath = Path.ChangeExtension(filepath, ".awb");
                    if (!File.Exists(awbPath))
                    {
                        _log.Info("[AcbPatcherEmulator] No AWB file found {0}", filepath);
                        _pathToStream.Remove(filepath);
                        return false;
                    }
                    
                    _ = new FileSlice(awbPath); // should open a handle, triggering AWB hook.
                    if (!_headerHashToHeader.TryGetValue(hash, out patcherEntry))
                    {
                        _log.Info("[AcbPatcherEmulator] No AWB entry found {0}", filepath);
                        _pathToStream.Remove(filepath);
                        return false;
                    }
                }
                    
                patcherEntry.WriteToAddress(afs2HeaderPtr);

                // Note: We will use MemoryStream for now as we haven't found a case where there are multiple files
                // and they are large enough to exceed 64K granularity.
                stream = new MemoryStream(data);
                _log.Info("[AcbPatcherEmulator] Overwritten ACB header in {0}", filepath);
                _awbToAcbMap[patcherEntry.AwbPath] = new AwbToAcbEntry(filepath, hash);
                
                if (DumpFiles)
                    Utility.DumpFile(_log, filepath, stream);
            }
        }
        finally
        {
            fileStream.Dispose();
            Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
        }
        
        _pathToStream[filepath] = stream;
        emulatedFile = new EmulatedFile<MemoryStream>(stream);
        return true;
    }

    public void UnregisterFile(string awbPath)
    {
        if (!_awbToAcbMap.Remove(awbPath, out var mapEntry)) 
            return;
        
        _pathToStream.Remove(mapEntry!.Value.AcbPath);
        _headerHashToHeader.Remove(mapEntry.Value.Hash);
    }
    
    internal record struct AwbToAcbEntry(string AcbPath, ulong Hash);
}