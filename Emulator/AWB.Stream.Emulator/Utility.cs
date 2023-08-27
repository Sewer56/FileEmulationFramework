using FileEmulationFramework.Lib.Utilities;

namespace AWB.Stream.Emulator;

internal class Utility
{
    internal static void DumpFile(Logger log, string filepath, global::System.IO.Stream stream)
    {
        var lastPosition = stream.Position;
        var filePath = Path.GetFullPath($"{Constants.DumpFolder}/{Path.GetFileName(filepath)}");
        Directory.CreateDirectory(Constants.DumpFolder);
        log.Info($"[AwbEmulator] Dumping {filepath}");
        using var fileStream = new FileStream(filePath, FileMode.Create);
        stream.CopyTo(fileStream);
        log.Info($"[AwbEmulator] Written To {filePath}");
        stream.Position = lastPosition;
    }
}