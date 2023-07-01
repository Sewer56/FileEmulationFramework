using static FileEmulationFramework.Lib.Utilities.Native;

namespace FileEmulationFramework.Utilities;

/// <summary>
/// Utilities for working with strings.
/// </summary>
internal class Strings
{
    /// <summary>
    /// Prefix used for sending file paths straight to filesystem.
    /// </summary>
    public const string PrefixNoParsingStr = @"\\?\";

    /// <summary>
    /// Prefix used by NT apis for devices and files.
    /// </summary>
    public const string PrefixLocalDeviceStr = @"\??\";

    /// <summary>
    /// Prefix used for devices only.
    /// </summary>
    public const string PrefixLocalDevice2Str = @"\\.\";

    // Note: Little Endian.
    private const long PrefixNoParsing = 0x5C003F005C005C; /* \\?\ */ // Send straight to filesystem.
    private const long PrefixLocalDevice = 0x5C003F003F005C; /* \??\ */ // Devices & files only
    private const long PrefixLocalDevice2 = 0x5C002E005C005C; /* \\.\ */ // Devices only

    /// <summary>
    /// Trims Windows NT file name prefixes from a given path.
    /// </summary>
    /// <param name="text">The <see cref="UNICODE_STRING"/> to trim.</param>
    public static unsafe string TrimWindowsPrefixes(UNICODE_STRING* text)
    {
        if (text->Length < 4)
            return text->ToString();

        var value = *(long*)text->Buffer;
        if (value is PrefixNoParsing or PrefixLocalDevice or PrefixLocalDevice2)
            return text->Substring(4);

        return text->ToString();
    }
}
