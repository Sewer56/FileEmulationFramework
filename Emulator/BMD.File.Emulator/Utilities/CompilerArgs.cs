using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlusScriptLibrary.MessageScriptLanguage;

namespace BMD.File.Emulator.Utilities;

public class CompilerArgs
{
    public string Library { get; set; }

    public string Encoding { get; set; }

    public string OutFormat { get; set; }

    public CompilerArgs(string library, string encoding, string outFormat)
    {
        Library = library;
        Encoding = encoding;
        OutFormat = outFormat;
    }

    public static FormatVersion GetMessageScriptFormatVersion(OutputFileFormat format)
    {
        FormatVersion version;
        switch (format)
        {
            case OutputFileFormat.V1:
                version = FormatVersion.Version1;
                break;
            case OutputFileFormat.V1BE:
                version = FormatVersion.Version1BigEndian;
                break;
            case OutputFileFormat.V1DDS:
                version = FormatVersion.Version1;
                break;
            case OutputFileFormat.BE:
                version = FormatVersion.BigEndian;
                break;
            default:
                version = FormatVersion.Unknown;
                break;
        }

        return version;
    }

    public enum OutputFileFormat
    {
        None,
        V1,
        V1DDS,
        BE,
        V1BE
    }
}
