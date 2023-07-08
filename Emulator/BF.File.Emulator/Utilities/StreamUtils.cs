using FileEmulationFramework.Lib.Memory;

namespace BF.File.Emulator.Utilities;
internal class StreamUtils
{
    /// <summary>
    /// Creates an appropriate Memory Stream for the specified length.
    /// </summary>
    /// <param name="length">The length of data that will be written to the stream.</param>
    /// <returns>A new <see cref="MemoryStream"/> if the length is less than the <see cref="Constants.AllocationGranularity"/>, 
    /// otherwise a new <see cref="MemoryManagerStream"/></returns>
    internal static Stream CreateMemoryStream(long length)
    {
        if (length < Constants.AllocationGranularity)
            return new MemoryStream();

        var manager = new MemoryManager(Constants.AllocationGranularity);
        return new MemoryManagerStream(manager);
    }
}
