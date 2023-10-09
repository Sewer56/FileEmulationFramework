using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.Win32.SafeHandles;
using Reloaded.Memory.Extensions;
using SPD.File.Emulator.Spd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.File.Emulator.Sprite
{
    public abstract class SpriteBuilder
    {
        protected Dictionary<string, FileSlice> _customSprFiles = new();
        protected Dictionary<string, FileSlice> _customTextureFiles = new();

        protected Logger _log;

        public SpriteBuilder(Logger log)
        {
            _log = log;
        }

        /// <summary>
        /// Adds a file to the Virtual SpriteFile builder.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        public abstract void AddOrReplaceFile(string filePath);

        /// <summary>
        /// Builds a Sprite file.
        /// </summary>
        public abstract unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0);

        internal static T GetHeaderFromSpr<T>(nint handle, long pos) where T : unmanaged
        {
            var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);

            stream.Seek(pos, SeekOrigin.Begin);

            try
            {
                return stream.Read<T>();
            }
            finally
            {
                stream.Dispose();
                _ = Native.SetFilePointerEx(handle, pos, IntPtr.Zero, 0);
            }
        }
    }
}
