using AtlusScriptLibrary.FlowScriptLanguage;

namespace BF.File.Emulator.Utilities;

public class CompilerArgs
{
    public string Library { get; set; }

    public string Encoding { get; set; }

    public string OutFormat { get; set; }

    public CompilerArgs(string library, string encoding, string outFormat) {
        Library = library;
        Encoding = encoding;
        OutFormat = outFormat;
    }

    public static FormatVersion GetFlowScriptFormatVersion(OutputFileFormat format)
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
                version = FormatVersion.Version1; // TODO: relay proper MessageScript version to FlowScript loader
                break;
            case OutputFileFormat.V2:
                version = FormatVersion.Version2;
                break;
            case OutputFileFormat.V2BE:
                version = FormatVersion.Version2BigEndian;
                break;
            case OutputFileFormat.V3:
                version = FormatVersion.Version3;
                break;
            case OutputFileFormat.V3BE:
                version = FormatVersion.Version3BigEndian;
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
        V1BE,
        V2,
        V2BE,
        V3,
        V3BE
    }
}
