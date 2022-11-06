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

    /// <summary>
    /// Registers a fake/virtual file whose access requests will be handled by the provided stream.
    /// A dummy file is placed in desired path (as a 0 byte file), such that it can be picked up by Windows Search APIs,
    /// then all requests to read that file will be redirected to the provided Stream instance.  
    /// </summary>
    /// <param name="filePath">Path to the file to be registered.</param>
    /// <param name="emulated">The stream from which the data for this file will be sourced.</param>
    /// <remarks>If file already exists, it will be overwritten.</remarks>
    public void RegisterVirtualFile(string filePath, IEmulatedFile emulate);
    
    /// <summary>
    /// Unregisters a virtual file registered with <see cref="RegisterVirtualFile"/>.
    /// </summary>
    /// <param name="filePath">Path to the file to be unregistered.</param>
    public void UnregisterVirtualFile(string filePath);
}