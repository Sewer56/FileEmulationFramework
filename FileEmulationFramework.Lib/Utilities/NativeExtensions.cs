namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Extensions over native functions.
/// </summary>
public static unsafe class NativeExtensions
{
    /// <summary>
    /// Reads data from the specified file or input/output (I/O) device. Reads occur at the position specified by the file pointer if
    /// supported by the device.
    /// </summary>
    /// <param name="hFile">
    ///     A handle to the device (for example, a file, file stream, physical disk, volume, console buffer, tape drive, socket,
    ///     communications resource, mailslot, or pipe). The hFile parameter must have been created with read access.
    /// </param>
    /// <param name="offset">Offset in file to read from.</param>
    /// <param name="lpBuffer">A pointer to the buffer that receives the data read from a file or device.</param>
    /// <param name="nNumberOfBytesToRead">The maximum number of bytes to be read.</param>
    /// <param name="numOfBytesRead">Number of bytes read by the function.</param>
    /// <returns>True if all bytes have been read, else false.</returns>
    public static bool TryReadFile(IntPtr hFile, long offset, byte* lpBuffer, uint nNumberOfBytesToRead, out uint numOfBytesRead)
    {
        numOfBytesRead = 0;
        uint numBytesToRead = nNumberOfBytesToRead;
        Native.SetFilePointerEx(hFile, offset, IntPtr.Zero, 0);

        do
        {
            bool success = Native.ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, out uint bytesRead, IntPtr.Zero);
            if (!success || bytesRead <= 0)
                return false;

            numOfBytesRead += bytesRead;
            numBytesToRead -= bytesRead;
        }
        while (numOfBytesRead < numBytesToRead);

        return true;
    }

}