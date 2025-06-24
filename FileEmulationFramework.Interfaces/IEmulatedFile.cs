﻿namespace FileEmulationFramework.Interfaces;

/// <summary>
/// This interface represents an individual file created by an emulator.
/// </summary>
public interface IEmulatedFile
{
    /// <summary>
    /// Gets the file size of a file with a given handle.
    /// </summary>
    /// <param name="handle">The handle of the file.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    /// <returns>Size of the emulated file.</returns>
    public long GetFileSize(IntPtr handle, IFileInformation info);

    /// <summary>
    /// Reads a specified number of bytes.
    /// </summary>
    /// <param name="handle">Handle to the file.</param>
    /// <param name="buffer">Pointer to the address that will receive the data.</param>
    /// <param name="length">Length of the data to be received.</param>
    /// <param name="offset">Offset of the data to be received.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    /// <param name="numReadBytes">Number of bytes read by the function call.</param>
    /// <returns>True if at least one byte was read, false otherwise (this indicates the EOF has been reached).</returns>
    public unsafe bool ReadData(IntPtr handle, byte* buffer, uint length, long offset, IFileInformation info, out int numReadBytes);
    
    /// <summary>
    /// Called when a handle to the given file is closed.
    /// </summary>
    /// <param name="handle">The handle of the file.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    public void CloseHandle(IntPtr handle, IFileInformation info);

    /// <summary>
    /// Tries to get the last write time of a file with a given handle.
    /// </summary>
    /// <param name="handle">The handle of the file.</param>
    /// <param name="info">Additional information about the current file being processed.</param>
    /// <param name="lastWriteTime">The time the emulated file was last written to.</param>
    /// <returns>True if the last write to was determined, false if the operation was not supported.</returns>
    public bool TryGetLastWriteTime(IntPtr handle, IFileInformation info, out DateTime? lastWriteTime);
}