using AWB.Stream.Emulator.Awb;
using AWB.Stream.Emulator.Awb.Structs;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.Sigscan.Definitions.Structs;

namespace AWB.Stream.Emulator.Acb;

/// <summary>
/// Class that can be used for patching of ACB files.
/// </summary>
public static class AcbPatcher
{
    /// <summary>
    /// Signature of AWB header.
    /// </summary>
    public static string AwbHeaderSignature = "41 46 53 32"; // AFS2

    /// <summary>
    /// Scans for AWB header inside ACB and injects the data.
    /// </summary>
    /// <param name="scannerFac">Creates scanners for data.</param>
    /// <param name="acbData">Data containing the ACB.</param>
    /// <param name="length">Length of the ACB data.</param>
    /// <param name="scanResult">The result of scanning for the header.</param>
    public static unsafe bool TryFindAwbHeader(IScannerFactory scannerFac, byte* acbData, int length, out PatternScanResult scanResult)
    {
        var scanner = scannerFac.CreateScanner(acbData, length);
        scanResult = scanner.FindPattern(AwbHeaderSignature);
        return scanResult.Found;
    }

    /// <summary>
    /// Scans for AWB header inside ACB and injects the data.
    /// </summary>
    /// <param name="scannerFac">Creates scanners for data.</param>
    /// <param name="acbData">Data containing the ACB.</param>
    /// <param name="length">Length of the ACB data.</param>
    /// <param name="afs2HeaderPtr">The pointer to AFS2/AWB header.</param>
    /// <param name="hash">The hash for the AWB header.</param>
    public static unsafe bool TryHashAwbHeader(IScannerFactory scannerFac, byte* acbData, int length, out byte* afs2HeaderPtr, out ulong hash)
    {
        if (!TryFindAwbHeader(scannerFac, acbData, length, out var pattern))
        {
            afs2HeaderPtr = (byte*)0;
            hash = 0;
            return false;
        }

        afs2HeaderPtr = acbData + pattern.Offset;
        hash = AwbHeaderReader.HashHeaderInRam((Afs2Header*)afs2HeaderPtr);
        return true;
    }
    
    /// <summary>
    /// Scans for AWB header inside ACB and injects the data.
    /// </summary>
    /// <param name="scannerFac">Creates scanners for data.</param>
    /// <param name="acbData">Data containing the ACB.</param>
    /// <param name="length">Length of the ACB data.</param>
    /// <param name="expectedHash">Expected hash, will replace if matches.</param>
    /// <param name="patcherEntry">Entry of the ACB patcher. Contains stream to AWB header.</param>
    public static unsafe bool TryInjectAwbHeader(IScannerFactory scannerFac, byte* acbData, int length, ulong expectedHash, AcbPatcherEntry patcherEntry)
    {
        if (!TryHashAwbHeader(scannerFac, acbData, length, out var afs2HeaderPtr, out ulong headerHash))
            return false;
        
        if (headerHash != expectedHash)
            return false;
        
        // Inject data.
        patcherEntry.WriteToAddress(afs2HeaderPtr);
        return true;
    }
}