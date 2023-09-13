using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.File.Emulator;

internal class Constants
{
    public const string SpdExtension = ".SPD";
    public const string TextureExtension = ".dds";
    public const string SpriteExtension = ".spdspr";
    public const string TextureEntryExtension = ".spdtex";
    public const string DumpFolder = "FEmulator-Dumps/SPDEmulator";
    public static readonly string RedirectorFolder = $"FEmulator{Path.DirectorySeparatorChar}SPD";
}
