using ONE.Heroes.Stream.Emulator.Interfaces;

namespace ONE.Heroes.Stream.Emulator;

/// <summary>
/// Registers programmatic inputs with a <see cref="OneEmulator"/>.
/// </summary>
public class OneEmulatorApi : IOneEmulator
{
    private readonly OneEmulator _emulator;

    /// <summary>
    /// Creates an API backed by the specified emulator.
    /// </summary>
    /// <param name="emulator">Emulator that receives registrations.</param>
    public OneEmulatorApi(OneEmulator emulator) => _emulator = emulator;

    /// <inheritdoc/>
    public void AddFile(string file, string route) => _emulator.AddFile(file, route);

    /// <inheritdoc/>
    public void AddDirectory(string dir) => _emulator.AddDirectory(dir);
}
