using FileEmulationFramework.Interfaces;

namespace FileEmulationFramework;

public class EmulationFramework : IEmulationFramework
{
    /// <inheritdoc/>
    public void Register(IEmulator emulator) => FileAccessServer.AddEmulator(emulator);

    /// <inheritdoc/>
    public void RegisterVirtualFile(string filePath, Stream stream) => FileAccessServer.RegisterVirtualFile(filePath, stream);
}