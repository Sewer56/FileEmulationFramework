using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Lib.Memory;
using AtlusScriptLibrary.FlowScriptLanguage.Compiler;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;
using AtlusScriptLibrary.Common.Libraries;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Reflection.Metadata;
using AtlusScriptLibrary.FlowScriptLanguage;
using System.IO;

namespace BF.File.Emulator.Bf
{
    public class BfBuilder
    {

        private readonly List<string> _flowFiles = new List<string>();

        /// <summary>
        /// Adds a file to the Virtual PAK builder.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        /// <param name="pakName">Name of the file inside the PAK.</param>
        public void AddFlowFile(string filePath)
        {
            if (!filePath.EndsWith(".flow", StringComparison.OrdinalIgnoreCase)) return;
            _flowFiles.Add(filePath);
        }

        /// <summary>
        /// Builds an BF file.
        /// </summary>
        public unsafe MemoryManagerStream? Build(IntPtr originalHandle, string originalPath, FlowFormatVersion flowFormat, Library library, Encoding encoding, AtlusLogListener? listener = null, Logger? logger = null, bool noBaseBf = false)
        {
            logger?.Info("[BfEmulator] Building BF File | {0}", originalPath);

            var compiler = new FlowScriptCompiler(flowFormat);
            compiler.Library = library;
            compiler.Encoding = encoding;
            compiler.ProcedureHookMode = ProcedureHookMode.ImportedOnly;
            if(listener != null)
                compiler.AddListener(listener);

            FileStream? bfStream = null;
            if(!noBaseBf)
                bfStream = new FileStream(new SafeFileHandle(originalHandle, false), FileAccess.Read);

            if (!compiler.TryCompileWithImports(bfStream, _flowFiles.GetRange(1,_flowFiles.Count-1), _flowFiles[0], out FlowScript flowScript))
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
    }
}
