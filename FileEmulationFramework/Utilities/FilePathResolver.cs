using System.Runtime.InteropServices;

namespace FileEmulationFramework.Utilities;

/// <summary>
/// Handy class for resolving symlinks in Windows.
/// </summary>
public static unsafe class FilePathResolver
{
    private const short MaxPath = short.MaxValue; // Windows 10 with path extension.
    
    [ThreadStatic]
    private static char[]? _pathBuffer;
    
    // by putting in pinned object heap, pinning is a no-op below
    private static char[] GetPathBuffer() => _pathBuffer ??= GC.AllocateUninitializedArray<char>(MaxPath, true);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern uint GetFinalPathNameByHandleW(IntPtr hFile, void* data, uint cchFilePath, uint dwFlags);

    /// <summary>
    /// Resolves a symbolic link and normalizes the path.
    /// </summary>
    /// <param name="handle">The handle to be resolved.</param>
    /// <param name="result">Resulting path name.</param>
    public static bool TryGetFinalPathName(IntPtr handle, out string result)
    {
        fixed (char* buffer = GetPathBuffer())
        {
            var res = GetFinalPathNameByHandleW(handle, buffer, (uint)MaxPath, 0);
            if (res == 0)
            {
                result = "";
                return false;
            }

            // Use GetFullPath to normalize returned path.
            result = RemoveDevicePrefix(new ReadOnlySpan<char>(buffer, (int)res));
            return true;
        }
    }

    private static string RemoveDevicePrefix(ReadOnlySpan<char> path)
    {
        const string devicePrefix = @"\\?\";
        if (path.StartsWith(devicePrefix))
            return path.Slice(devicePrefix.Length).ToString();

        return path.ToString();
    }
}