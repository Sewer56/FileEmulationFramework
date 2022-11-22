using System.Runtime.CompilerServices;

namespace AWB.Stream.Emulator.Awb.Utilities;

/// <summary>
/// Class used to help throwing exceptions.
/// </summary>
internal static class ThrowHelpers
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNotAfs2ArchiveException() => throw new Exception($"This is not an AFS2/AWB archive, 'AFS2' magic was not found.");
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidArchiveTypeException(int type) => throw new Exception($"Invalid archive type {type}, only Type 1 & Type 2 are supported.");
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowBadFieldSizeException() => throw new Exception($"Invalid field size for AWB element. Supports only 8/16/32/64 bit");
}