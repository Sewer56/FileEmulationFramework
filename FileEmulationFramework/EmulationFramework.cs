using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework;

public class EmulationFramework : IEmulationFramework
{
    /// <inheritdoc/>
    public void Register(IEmulator emulator) => FileAccessServer.AddEmulator(emulator);

    /// <inheritdoc/>
    public void RegisterVirtualFile(string filePath, IEmulatedFile emulated) => FileAccessServer.RegisterVirtualFile(filePath, emulated);

    /// <inheritdoc/>
    public void RegisterVirtualFile(string filePath, IEmulatedFile emulate, bool overwrite) => FileAccessServer.RegisterVirtualFile(filePath, emulate, overwrite);

    /// <inheritdoc/>
    public void UnregisterVirtualFile(string filePath) => FileAccessServer.UnregisterVirtualFile(filePath);

    /// <inheritdoc/>
    public void UnregisterVirtualFile(string filePath, bool delete) => FileAccessServer.UnregisterVirtualFile(filePath, delete);
}