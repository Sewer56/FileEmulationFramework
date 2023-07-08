// ReSharper disable InconsistentNaming
namespace PAK.Stream.Emulator.Pak;

public enum FormatVersion
{
    /// <summary>
    /// 252 bytes filename, 4 bytes filesize
    /// </summary>
    Version1,

    /// <summary>
    /// Entry count header, 32 bytes filename, 4 bytes filesize
    /// </summary>
    Version2,

    /// <summary>
    /// <see cref="Version2"/> with Big Endian
    /// </summary>
    Version2BE,

    /// <summary>
    /// Entry count header, 24 bytes filename, 4 bytes filesize
    /// </summary>
    Version3,

    /// <summary>
    /// <see cref="Version3"/> with Big Endian
    /// </summary>
    Version3BE,

    Unknown
}