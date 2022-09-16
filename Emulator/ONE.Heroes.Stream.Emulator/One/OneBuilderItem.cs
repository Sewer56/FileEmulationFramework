// Aliasing for readability, since our assembly name has priority over 'stream'
using Strim = System.IO.Stream;

namespace ONE.Heroes.Stream.Emulator.One;

/// <summary>
/// Represents an item used by the builder.
/// </summary>
public class OneBuilderItem
{
    /// <summary>
    /// Stream that backs this file, must support read & length, and be PRS compressed.
    /// </summary>
    public Strim Stream;

    /// <summary>
    /// Name of the file.
    /// </summary>
    public string Name;

    public OneBuilderItem(Strim stream, string name)
    {
        Stream = stream;
        Name = name;
    }
}