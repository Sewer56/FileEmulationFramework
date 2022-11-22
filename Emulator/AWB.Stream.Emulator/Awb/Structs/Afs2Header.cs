using AWB.Stream.Emulator.Awb.Utilities;
using FileEmulationFramework.Lib.Utilities;
using ThrowHelpers = AWB.Stream.Emulator.Awb.Utilities.ThrowHelpers;

namespace AWB.Stream.Emulator.Awb.Structs;

/// <summary>
/// Header of the AFS2 file.
/// </summary>
public struct Afs2Header
{
    /// <summary>
    /// Expected Signature of the archive 'AFS2'.
    /// </summary>
    public const int ExpectedMagic = 0x32534641;
    
    /// <summary>
    /// Signature of the archive, should be 'AFS2'.
    /// </summary>
    public int Magic;

    /// <summary>
    /// Type of AFS2 file, only '1' and '2' are known.
    /// </summary>
    public byte Type;
    
    /// <summary>
    /// Length of the field storing file positions.
    /// </summary>
    public byte PositionFieldLength;
    
    /// <summary>
    /// Length of the field storing file indices.
    /// </summary>
    public byte IdFieldLength;
    
#pragma warning disable CS0169
    private byte _pad;
#pragma warning restore CS0169
    
    /// <summary>
    /// Number of entries in this archive.
    /// </summary>
    public int EntryCount;
    
    /// <summary>
    /// Alignment of files in this archive, in bytes.
    /// </summary>
    public short Alignment;
    
    /// <summary>
    /// a.k.a. subkey.
    /// </summary>
    public short EncryptionKey;
    
    /// <summary>
    /// Retrieves the complete size of the file header, excluding padding.
    /// </summary>
    public int GetTotalSizeOfHeader() => GetTotalSizeOfHeader(IdFieldLength, EntryCount, PositionFieldLength);

    /// <summary>
    /// Retrieves the complete size of the file header, excluding padding.
    /// </summary>
    public static unsafe int GetTotalSizeOfHeader(int idFieldLength, int entryCount, int positionFieldLength)
    {
        var baseSize = sizeof(Afs2Header);
        baseSize += (idFieldLength + positionFieldLength) * entryCount;
        baseSize += positionFieldLength;
        return baseSize;
    }
    
    /// <summary>
    /// Retrieves the complete size of the file header, including padding.
    /// </summary>
    public static int GetTotalSizeOfHeaderWithPadding(int idFieldLength, int entryCount, int positionFieldLength, int alignment)
    {
        var size = GetTotalSizeOfHeader(idFieldLength, entryCount, positionFieldLength);
        return Mathematics.RoundUp(size, alignment);
    }

    /// <summary>
    /// Retrieves the size of an individual entry in bytes.
    /// </summary>
    public int GetSizeOfEntryBytes() => IdFieldLength + PositionFieldLength;

    /// <summary>
    /// Reads an id field for an entry from memory using the size specified in file header.
    /// </summary>
    /// <param name="data">Pointer to the individual id field in memory. Will be incremented to next entry.</param>
    /// <returns>Individual id field, extended.</returns>
    public unsafe long ReadIdFieldAndIncrementPtr(ref byte* data) => ValueReaders.ReadNumberAndIncrementPtr(ref data, IdFieldLength);

    /// <summary>
    /// Reads a position field from memory using the size specified in file header.
    /// </summary>
    /// <param name="data">Pointer to the individual position in memory. Will be incremented to next entry.</param>
    /// <returns>Individual position field, extended.</returns>
    public unsafe long ReadPositionAndIncrementPtr(ref byte* data) => ValueReaders.ReadNumberAndIncrementPtr(ref data, PositionFieldLength);

    /// <summary>
    /// Throws if an unsupported type is found.
    /// </summary>
    public void AssertSupportedArchive()
    {
        if (Magic != ExpectedMagic)
            ThrowHelpers.ThrowNotAfs2ArchiveException();
        
        if (Type != 1 && Type != 2)
            ThrowHelpers.ThrowInvalidArchiveTypeException(Type);
    }
}