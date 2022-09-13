namespace FileEmulationFramework.Interfaces;

/// <summary>
/// APIs powering the emulation framework as a whole.
/// </summary>
public interface IEmulationFramework
{
    /// <summary>
    /// Registers a specific emulator.
    /// </summary>
    /// <param name="emulator">The emulator to register.</param>
    public void Register(IEmulator emulator);
}