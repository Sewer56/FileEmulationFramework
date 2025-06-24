namespace BMD.File.Emulator.Bmd;

/// <summary>
/// Contains information about a BMD file that has been emulated
/// </summary>
public class EmulatedBmd
{
    /// <summary>
    /// A list of full paths to all source files used to compile this bmd.
    /// This includes all msg files but excludes the base bmd.
    /// </summary>
    public List<string> Sources { get; }

    /// <summary>
    /// The stream for the emulated file
    /// </summary>
    public Stream Stream { get; }
    
    /// <summary>
    /// The last write time of the file
    /// This is set as the maximum last write time of all sources when the file was first emulated
    /// </summary>
    public DateTime LastWriteTime { get; }
    
    public EmulatedBmd(Stream stream, List<String> sources, DateTime lastWriteTime)
    {
        Sources = sources;
        Stream = stream;
        LastWriteTime = lastWriteTime;
    }
}