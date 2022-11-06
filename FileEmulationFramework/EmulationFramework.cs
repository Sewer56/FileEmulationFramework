using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework;

public class EmulationFramework : IEmulationFramework
{
    /// <inheritdoc/>
    public void Register(IEmulator emulator) => FileAccessServer.AddEmulator(emulator);

    /// <inheritdoc/>
    public void RegisterVirtualFile(string filePath, IEmulatedFile emulated) => FileAccessServer.RegisterVirtualFile(filePath, emulated);

    /// <inheritdoc/>
    public void UnregisterVirtualFile(string filePath) => FileAccessServer.UnregisterVirtualFile(filePath);
}