using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework;

// Note: This is a wrapper around the list; it is intended for this to be copied by value (for now)
// as it will be more performant, avoiding an extra pointer dereference.
public struct EmulationFramework : IEmulationFramework
{
    /// <summary>
    /// The emulators currently held by the framework.
    /// </summary>
    public List<IEmulator> Emulators { get; private set; } = new();

    public EmulationFramework() { }

    /// <summary>
    /// Registers a specific emulator.
    /// </summary>
    /// <param name="emulator">The emulator to register.</param>
    public void Register(IEmulator emulator) => Emulators.Add(emulator);
}