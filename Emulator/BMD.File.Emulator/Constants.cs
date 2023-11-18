namespace BMD.File.Emulator;

internal class Constants
{
    public const string BmdExtension = ".bmd";
    public const string MessageExtension = ".msg";
    public const string DumpExtension = "dmp.bmd";
    public const string DumpFolder = "FEmulator-Dumps/BMDEmulator";
    public const string ArgsFile = "CompilerArgs.json";
    public static readonly string RedirectorFolder = $"FEmulator{Path.DirectorySeparatorChar}BMD";
    public const int AllocationGranularity = 65536;
}
