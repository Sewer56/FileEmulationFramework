using FileEmulationFramework.Lib.Utilities;
using AtlusScriptLibrary.FlowScriptLanguage.Compiler;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;
using AtlusScriptLibrary.Common.Libraries;
using System.Text;
using Microsoft.Win32.SafeHandles;
using AtlusScriptLibrary.FlowScriptLanguage;
using System.Text.Json;
using BF.File.Emulator.Utilities;

// Aliasing for readability, since our assembly name has priority over 'File'
using Fiel = System.IO.File;

namespace BF.File.Emulator.Bf;

public class BfBuilder
{

    private readonly List<string> _flowFiles = new List<string>();
    private readonly List<string> _msgFiles = new List<string>();
    private readonly List<FlowScriptModuleFunction> _libraryFuncs = new List<FlowScriptModuleFunction>();
    private readonly List<FlowScriptModuleEnum> _libraryEnums = new List<FlowScriptModuleEnum>();
    private readonly HashSet<string> _addedOverrides = new();
    
    private FlowFormatVersion? _flowFormat = null;
    private Library? _library = null;
    private Encoding? _encoding = null;
    private Logger? _log = null;

    public BfBuilder(FlowFormatVersion? flowFormat, Library? library, Encoding? encoding, Logger? log)
    {
        _flowFormat = flowFormat;
        _library = library;
        _encoding = encoding;
        _log = log;
    }

    public BfBuilder() { }

    /// <summary>
    /// Adds a flow file that will be imported when compiling the bf
    /// </summary>
    /// <param name="filePath">Full path to the bf file.</param>
    public void AddFlowFile(string filePath)
    {
        if (!filePath.EndsWith(Constants.FlowExtension, StringComparison.OrdinalIgnoreCase)) return;
        _flowFiles.Add(filePath);
    }

    /// <summary>
    /// Adds a msg file that will be imported when compiling the bf
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public void AddMsgFile(string filePath)
    {
        if (!filePath.EndsWith(Constants.MessageExtension, StringComparison.OrdinalIgnoreCase)) return;
        // If there's a flow with the same name imported then this isn't actually a message hook, likely an import from that flow
        if (_flowFiles.Contains(Path.ChangeExtension(filePath, Constants.FlowExtension))) return;
        _msgFiles.Add(filePath);
    }

    /// <summary>
    /// Adds a library file that will overwrite script compiler libraries
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <returns>True if the library file could be addeed, false otherwise</returns>
    public void AddLibraryFile(string filePath)
    {
        if (!filePath.EndsWith(Constants.JsonExtension, StringComparison.OrdinalIgnoreCase) || _addedOverrides.Contains(filePath)) return;

        string jsonText = Fiel.ReadAllText(filePath);
        var functions = JsonSerializer.Deserialize<List<FlowScriptModuleFunction>>(jsonText);
        _addedOverrides.Add(filePath);
        if (functions == null)
        {
            _log?.Info($"[BfBuilder] Failed to add library function overrides from {filePath}");
        }
        else
        {
            _libraryFuncs.AddRange(functions);
            _log?.Info($"[BfBuilder] Added library function overrides from {filePath}");
        }
    }

    /// <summary>
    /// Adds a enums file that will overwrite script compiler enums
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <returns>True if the enum file could be addeed, false otherwise</returns>
    public void AddEnumFile(string filePath)
    {
        if (!filePath.EndsWith(Constants.JsonExtension, StringComparison.OrdinalIgnoreCase) || _addedOverrides.Contains(filePath)) return;

        string jsonText = Fiel.ReadAllText(filePath);
        var enums = JsonSerializer.Deserialize<List<FlowScriptModuleEnum>>(jsonText);
        _addedOverrides.Add(filePath);
        if (enums == null)
        {
            _log?.Info($"[BfBuilder] Failed to add library enum overrides from {filePath}");
        }
        else
        {
            _libraryEnums.AddRange(enums);
            _log?.Info($"[BfBuilder] Added library enum overrides from {filePath}");
        }
    }

    /// <summary>
    /// Tries to get all files that the base flow imports
    /// </summary>
    /// <param name="flowFormat">The format of the flowscript</param>
    /// <param name="library">The library to use for the flowscript</param>
    /// <param name="encoding">The encoding of the flowscript</param>
    /// <param name="foundImports">An array of absolute paths to all files that the base flows import,
    /// both directly and transitively. This includes the base files.</param>
    /// <returns></returns>
    public bool TryGetImports(FlowFormatVersion flowFormat, Library library, Encoding encoding,
        out string[] foundImports)
    {
        // Use compiler arg overrides (if they're there)
        if (_library != null) library = _library;
        if (_encoding != null) encoding = _encoding;
        if (_flowFormat != null) flowFormat = (FlowFormatVersion)_flowFormat;

        var compiler = new FlowScriptCompiler(flowFormat);
        compiler.Library = OverrideLibraries(library);
        compiler.Encoding = encoding;

        var imports = new List<string>();
        imports.AddRange(_flowFiles);
        imports.AddRange(_msgFiles);

        return compiler.TryGetImports(imports, out foundImports);
    }

    /// <summary>
    /// Builds a BF file.
    /// </summary>
    public EmulatedBf? Build(IntPtr originalHandle, string originalPath, FlowFormatVersion flowFormat, Library library, Encoding encoding, AtlusLogListener? listener = null, bool noBaseBf = false)
    {
        _log?.Info("[BfEmulator] Building BF File | {0}", originalPath);

        // Use compiler arg overrides (if they're there)
        if (_library != null) library = _library;
        if(_encoding != null) encoding = _encoding;
        if (_flowFormat != null) flowFormat = (FlowFormatVersion)_flowFormat;

        var compiler = new FlowScriptCompiler(flowFormat);
        compiler.Library = OverrideLibraries(library);
        compiler.Encoding = encoding;
        compiler.ProcedureHookMode = ProcedureHookMode.ImportedOnly;
        compiler.OverwriteExistingMsgs = true;
        if (listener != null)
            compiler.AddListener(listener);

        FileStream? bfStream = null;
        if (!noBaseBf)
            bfStream = new FileStream(new SafeFileHandle(originalHandle, false), FileAccess.Read);

        var baseFlow = _flowFiles.Count > 0 ? _flowFiles[0] : null;
        var imports = new List<string>();
        if (_flowFiles.Count > 0)
            imports.AddRange(_flowFiles.GetRange(1, _flowFiles.Count - 1));
        imports.AddRange(_msgFiles);

        try
        {
            if (!compiler.TryCompileWithImports(bfStream, imports, baseFlow, out FlowScript flowScript,
                    out var sources))
            {
                _log?.Error("[BfEmulator] Failed to compile BF File | {0}", originalPath);
                return null;
            }

            // Return the compiled bf
            var bfBinary = flowScript.ToBinary();
            var stream = StreamUtils.CreateMemoryStream(bfBinary.Header.FileSize);
            bfBinary.ToStream(stream, true);
            stream.Position = 0;

            DateTime lastWrite = sources.Where(x => x != null).Select(Fiel.GetLastWriteTimeUtc).Max();
            return new EmulatedBf(stream, sources, lastWrite);
        }
        catch (Exception exception)
        {
            var flows = string.Join(", ", _flowFiles.Concat(_msgFiles));
            _log.Error(
                "[BF Builder] Failed to compile bf {0} with source files: {1}. This may be due to your mods not being translated. Error: {2}",
                originalPath, flows, exception.Message);
            if (exception.StackTrace != null)
                _log.Error(exception.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Applies library file overrides to the base library.
    /// This adds aliases to functions with different names, replaces functions with different
    /// return values or parameters, and adds any completely new functions.
    /// </summary>
    /// <param name="library">The base library to override</param>
    /// <returns>A deep copy of the base library with overrides applied</returns>
    private Library OverrideLibraries(Library library)
    {
        if (_libraryEnums.Count == 0 && _libraryFuncs.Count == 0) return library;

        // Clone library since every bf builder uses the same one
        library = (Library)library.Clone();

        // Override existing functions
        foreach (var func in _libraryFuncs)
        {
            int module = -1;
            int index = -1;

            for (int i = 0; i < library.FlowScriptModules.Count; i++)
            {
                index = library.FlowScriptModules[i].Functions.FindIndex(x => x.Index == func.Index);

                if (index != -1)
                {
                    module = i;
                    break;
                }
            }

            if (module != -1 && index != -1)
            {
                var existingFunc = library.FlowScriptModules[module].Functions[index];
                if (FlowFunctionsSame(existingFunc, func))
                {
                    existingFunc.Aliases ??= new List<string>();
                    existingFunc.Aliases.Add(func.Name);
                }
                else
                {
                    library.FlowScriptModules[module].Functions[index] = func;
                }
            }
            else
            {
                library.FlowScriptModules.Last().Functions.Add(func);
            }
        }

        // Add enums
        library.FlowScriptModules[0].Enums.AddRange(_libraryEnums);
        return library;
    }

    /// <summary>
    /// Checks if two flowscript functions are effectively the same.
    /// They are the same if the return type and parameter types are the same.
    /// </summary>
    /// <param name="func1">The first function to compare</param>
    /// <param name="func2">The other function to compare</param>
    /// <returns>Truee if the two functions are effectively the same, false otherwise</returns>
    private bool FlowFunctionsSame(FlowScriptModuleFunction func1, FlowScriptModuleFunction func2)
    {
        return func1.ReturnType == func2.ReturnType && func1.Parameters.Select(param => param.Type)
            .SequenceEqual(func2.Parameters.Select(param => param.Type));
    }
}