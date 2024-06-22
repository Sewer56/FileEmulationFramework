using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Lib.Memory;
using AtlusScriptLibrary.MessageScriptLanguage.Compiler;
using MessageFormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;
using AtlusScriptLibrary.Common.Libraries;
using System.Text;
using Microsoft.Win32.SafeHandles;
using AtlusScriptLibrary.MessageScriptLanguage;
using System.Text.Json;
using BMD.File.Emulator.Utilities;

// Aliasing for readability, since our assembly name has priority over 'File'
using Fiel = System.IO.File;

namespace BMD.File.Emulator.Bmd;

public class BmdBuilder
{
    private readonly List<string> _msgFiles = new List<string>();
    private readonly HashSet<string> _addedOverrides = new();

    private MessageFormatVersion? _messageFormat = null;
    private Library? _library = null;
    private Encoding? _encoding = null;
    private Logger? _log = null;

    public BmdBuilder(MessageFormatVersion? messageFormat, Library? library, Encoding? encoding, Logger? log)
    {
        _messageFormat = messageFormat;
        _library = library;
        _encoding = encoding;
        _log = log;
    }

    public BmdBuilder() { }

    /// <summary>
    /// Adds a msg file that will be imported when compiling the bmd
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public void AddMsgFile(string filePath)
    {
        if (!filePath.EndsWith(Constants.MessageExtension, StringComparison.OrdinalIgnoreCase)) return;
        _msgFiles.Add(filePath);
    }

    /// <summary>
    /// Builds a BMD file.
    /// </summary>
    public unsafe Stream? Build(IntPtr originalHandle, string originalPath, MessageFormatVersion msgFormat, Library library, Encoding encoding, AtlusLogListener? listener = null, bool noBaseBmd = false)
    {
        _log?.Info("[BmdEmulator] Building BMD File | {0}", originalPath);

        // Use compiler arg overrides (if they're there)
        if (_library != null) library = _library;
        if (_encoding != null) encoding = _encoding;
        if (_messageFormat != null) msgFormat = (MessageFormatVersion)_messageFormat;

        var compiler = new MessageScriptCompiler(msgFormat, encoding);
        compiler.Library = library;
        compiler.OverwriteExistingMsgs = true;
        if (listener != null)
            compiler.AddListener(listener);

        FileStream? bmdStream = null;
        if (!noBaseBmd)
            bmdStream = new FileStream(new SafeFileHandle(originalHandle, false), FileAccess.Read);

        if (!compiler.TryCompileWithImports(bmdStream, _msgFiles, out MessageScript messageScript))
        {
            _log?.Error("[BmdEmulator] Failed to compile BMD File | {0}", originalPath);
            return null;
        }

        // Return the compiled bmd
        var bmdBinary = messageScript.ToBinary();
        var stream = StreamUtils.CreateMemoryStream(bmdBinary.Header.FileSize);
        bmdBinary.ToStream(stream, true);
        stream.Position = 0;

        return stream;
    }
}
