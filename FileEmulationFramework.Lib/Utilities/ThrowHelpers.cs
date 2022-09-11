using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileEmulationFramework.Lib.Utilities;

internal static class ThrowHelpers
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ObjectDisposed(string message) => throw new ObjectDisposedException(message);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentException(string message) => throw new ArgumentException(message);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Win32Exception(string message) => throw new Win32Exception(message);
}