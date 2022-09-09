using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileEmulationFramework.Lib.Utilities;

internal static class ThrowHelpers
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposed(string message) => throw new ObjectDisposedException(message);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowApiCallFailed(string message) => throw new Win32Exception(message);
}