using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Lib.Memory;
using AtlusScriptLibrary.FlowScriptLanguage.Compiler;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;
using AtlusScriptLibrary.Common.Libraries;
using System.Text;
using Microsoft.Win32.SafeHandles;
using AtlusScriptLibrary.FlowScriptLanguage;
using Newtonsoft.Json;

// Aliasing for readability, since our assembly name has priority over 'File'
using Fiel = System.IO.File;


namespace BF.File.Emulator.Bf
{
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

        public BfBuilder(FlowFormatVersion? flowFormat, Library? library, Encoding? encoding)
        {
            _flowFormat = flowFormat;
            _library = library;
            _encoding = encoding;
        }

        public BfBuilder() { }

        /// <summary>
        /// Adds a flow file that will be imported when compiling the bf
        /// </summary>
        /// <param name="filePath">Full path to the bf file.</param>
        public void AddFlowFile(string filePath)
        {
            if (!filePath.EndsWith(".flow", StringComparison.OrdinalIgnoreCase)) return;
            _flowFiles.Add(filePath);
        }

        /// <summary>
        /// Adds a msg file that will be imported when compiling the bf
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        public void AddMsgFile(string filePath)
        {
            if (!filePath.EndsWith(".msg", StringComparison.OrdinalIgnoreCase)) return;
            // If there's a flow with the same name imported then this isn't actually a message hook, likely an import from that flow
            if (_flowFiles.Contains(Path.ChangeExtension(filePath, ".flow"))) return;
            _msgFiles.Add(filePath);
        }

        /// <summary>
        /// Adds a library file that will overwrite script compiler libraries
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        /// <returns>True if the library file could be addeed, false otherwise</returns>
        public void AddLibraryFile(string filePath, Logger? log = null)
        {
            if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || _addedOverrides.Contains(filePath)) return;

            string jsonText = Fiel.ReadAllText(filePath);
            var functions = JsonConvert.DeserializeObject<List<FlowScriptModuleFunction>>(jsonText);
            _addedOverrides.Add(filePath);
            if (functions == null)
            {
                log?.Info($"[BfBuilder] Failed to add library function overrides from {filePath}");
            }
            else
            {
                _libraryFuncs.AddRange(functions);
                log?.Info($"[BfBuilder] Added library function overrides from {filePath}");
            }
        }

        /// <summary>
        /// Adds a enums file that will overwrite script compiler enums
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        /// <returns>True if the enum file could be addeed, false otherwise</returns>
        public void AddEnumFile(string filePath, Logger? log = null)
        {
            if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || _addedOverrides.Contains(filePath)) return;

            string jsonText = Fiel.ReadAllText(filePath);
            var enums = JsonConvert.DeserializeObject<List<FlowScriptModuleEnum>>(jsonText);
            _addedOverrides.Add(filePath);
            if (enums == null)
            {
                log?.Info($"[BfBuilder] Failed to add library enum overrides from {filePath}");
            }
            else
            {
                _libraryEnums.AddRange(enums);
                log?.Info($"[BfBuilder] Added library enum overrides from {filePath}");
            }
        }

        /// <summary>
        /// Builds an BF file.
        /// </summary>
        public unsafe MemoryManagerStream? Build(IntPtr originalHandle, string originalPath, FlowFormatVersion flowFormat, Library library, Encoding encoding, AtlusLogListener? listener = null, Logger? logger = null, bool noBaseBf = false)
        {
            logger?.Info("[BfEmulator] Building BF File | {0}", originalPath);

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

            if (!compiler.TryCompileWithImports(bfStream, imports, baseFlow, out FlowScript flowScript))
            {
                logger?.Error("[BfEmulator] Failed to compile BF File | {0}", originalPath);
                return null;
            }

            var memoryManager = new MemoryManager(65536);
            var memoryManagerStream = new MemoryManagerStream(memoryManager);
            flowScript.ToStream(memoryManagerStream, true);
            memoryManagerStream.Position = 0;

            return memoryManagerStream;
        }

        private Library OverrideLibraries(Library library)
        {
            if (_libraryEnums.Count == 0 && _libraryFuncs.Count == 0) return library;

            // Clone library since every bf builder uses the same one
            library = (Library)library.Clone();

            // Override existing functions
            foreach (var func in _libraryFuncs)
            {
                for (int i = 0; i < library.FlowScriptModules.Count; i++)
                {
                    var existing = library.FlowScriptModules[i].Functions.FindIndex(x => x.Index == func.Index);
                    if (existing != -1)
                        library.FlowScriptModules[i].Functions[existing] = func;
                }
            }

            // Add enums
            library.FlowScriptModules[0].Enums.AddRange(_libraryEnums);
            return library;
        }
    }
}
