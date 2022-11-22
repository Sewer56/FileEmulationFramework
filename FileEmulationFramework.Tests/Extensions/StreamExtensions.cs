using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileEmulationFramework.Tests.Extensions;

public static class StreamExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<T> stackSpace = stackalloc T[1];
        _ = stream.Read(MemoryMarshal.Cast<T, byte>(stackSpace));
        return stackSpace[0];
    }
}