namespace SPD.File.Emulator;

internal class Constants
{
    public const string SpdExtension = ".spd";
    public const string SpdTextureExtension = ".dds";
    public const string SprTextureExtension = ".tmx";
    public const string SpdSpriteExtension = ".spdspr";
    public const string SprSpriteExtension = ".sprt";
    public const string DumpFolder = "FEmulator-Dumps/SPDEmulator";
    public static readonly string RedirectorFolder = $"FEmulator{Path.DirectorySeparatorChar}SPD";
    public const int AllocationGranularity = 65536;
}
