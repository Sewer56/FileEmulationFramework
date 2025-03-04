namespace BF.File.Emulator.Bf;

/// <summary>
/// Contains information about a BF file that has been emulated
/// </summary>
public class EmulatedBf
{
    /// <summary>
    /// A list of full paths to all source files used to compile this bf.
    /// This includes all msg, bf, and flow files that are imported excluding the base bf.
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

    public EmulatedBf(Stream stream, List<String> sources, DateTime lastWriteTime)
    {
        Sources = sources;
        Stream = stream;
        LastWriteTime = lastWriteTime;
    }
}