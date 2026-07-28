using AFS.Stream.Emulator.Interfaces;

namespace AFS.Stream.Emulator;

/// <summary>
/// Registers programmatic inputs with an <see cref="AfsEmulator"/>.
/// </summary>
public class AfsEmulatorApi : IAfsEmulator
{
    private readonly AfsEmulator _emulator;

    /// <summary>
    /// Creates an API backed by the specified emulator.
    /// </summary>
    /// <param name="emulator">Emulator that receives registrations.</param>
    public AfsEmulatorApi(AfsEmulator emulator) => _emulator = emulator;

    /// <inheritdoc/>
    public void AddFile(string file, string route, int index)
    {
        if ((uint)index > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(index), index, "AFS indexes must be between 0 and 65535.");

        _emulator.AddFile(file, route, index);
    }

    /// <inheritdoc/>
    public void AddDirectory(string dir) => _emulator.AddDirectory(dir);
}
