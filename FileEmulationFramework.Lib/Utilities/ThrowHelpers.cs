using System.ComponentModel;
using System.Runtime.CompilerServices;
// ReSharper disable InconsistentNaming

namespace FileEmulationFramework.Lib.Utilities;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Classes used to help with throwing exceptions in a performance efficient manner.
/// </summary>
public static class ThrowHelpers
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void IO(string message) => throw new IOException(message);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ObjectDisposed(string message) => throw new ObjectDisposedException(message);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Argument(string message) => throw new ArgumentException(message);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Win32(string message) => throw new Win32Exception(message);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member