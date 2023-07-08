
namespace BF.File.Emulator;

internal class Constants
{
    public const string BfExtension = ".bf";
    public const string FlowExtension = ".flow";
    public const string MessageExtension = ".msg";
    public const string JsonExtension = ".json";
    public const string DumpExtension = "dmp.bf";
    public const string DumpFolder = "FEmulator-Dumps/BFEmulator";
    public const string FunctionsFile = "Functions.json";
    public const string EnumsFile = "Enums.json";
    public const string ArgsFile = "CompilerArgs.json";
    public static readonly string RedirectorFolder = $"FEmulator{Path.DirectorySeparatorChar}BF";
    public const int AllocationGranularity = 65536;
}
