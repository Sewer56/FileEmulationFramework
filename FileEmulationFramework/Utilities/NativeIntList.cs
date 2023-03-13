using System.Runtime.InteropServices;

namespace FileEmulationFramework.Utilities;

/// <summary>
/// An implementation of a list of nints stored in native memory.
/// </summary>
[StructLayout(LayoutKind.Sequential)] // consistent between builds
public unsafe struct NativeIntList
{
    /*
        This is an implementation that directly translates to ASM used in CloseHandleHook in FileAccessServer.

        We get output from DOTNET_JitDisasm [easiest way to do via Disasmo Alt+Shift+D] and run the following regex.

        Find: "^       \S*\s*"
        Replace: ""

        Find: "word ptr"
        Replace: "word"
    
        Find: "bword" // 'byref word'
        Replace: "qword" or "dword"

        Find: "G_M000"
        Replace: "<FUNCTION_NAME>"

        [OPTIONAL]
        Find: "^align.*$\n" // in LF mode for newlines
        Replace: ""
        or 
        Replace: "align 16\n"

        Find: ";.*" // Strips comments
        Replace: ""
        
        Find: "[^\S\r\n]+" // Whitespace
        Replace: " " // Saves on RAM by storing less in constant string.

        Need manually remove CORINFO_HELP_POLL_GC calls.
        Test in FASMW
     */

    public const int DefaultThreadHandle = -1;
    
    /// <summary>
    /// Function used to allocate more memory.
    /// </summary>
    public delegate*unmanaged[Cdecl, SuppressGCTransition]<nuint, nuint> Malloc;
    
    /// <summary>
    /// Function used to allocate more memory.
    /// </summary>
    public delegate*unmanaged[Cdecl, SuppressGCTransition]<nuint, void> Free;
    
    /// <summary>
    /// Function used to get current thread ID.
    /// </summary>
    public delegate*unmanaged[Stdcall, SuppressGCTransition]<int> GetCurrentThreadId;
    
    /// <summary>
    /// Pointer to the first element of the list.
    /// </summary>
    public nint* ListPtr;
    
    /// <summary>
    /// Number of items in list.
    /// </summary>
    public int NumItems;

    /// <summary>
    /// Size of list.
    /// </summary>
    public int Capacity;
    
    /// <summary>
    /// Handle of current thread.
    /// </summary>
    public int CurrentThread;

    public NativeIntList()
    {
        var ucrt = Lib.Utilities.Native.LoadLibrary("ucrtbase.dll");
        Malloc = (delegate*unmanaged[Cdecl, SuppressGCTransition]<nuint, nuint>)Lib.Utilities.Native.GetProcAddress(ucrt, "malloc");
        Free = (delegate*unmanaged[Cdecl, SuppressGCTransition]<nuint, void>)Lib.Utilities.Native.GetProcAddress(ucrt, "free");
        
        var kernel32 = Lib.Utilities.Native.LoadLibrary("kernel32.dll");
        GetCurrentThreadId = (delegate*unmanaged[Stdcall, SuppressGCTransition]<int>)Lib.Utilities.Native.GetProcAddress(kernel32, "GetCurrentThreadId");
        
        NumItems = 0;
        Capacity = 20;
        ListPtr  = (nint*)Malloc((nuint)(Capacity * sizeof(nuint)));
        CurrentThread = DefaultThreadHandle;
    }

    /*
    // ASM Only
     
    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void AddItem(nint value)
    {
        var threadId = GetCurrentThreadId();
        while (Interlocked.CompareExchange(ref CurrentThread, threadId, DefaultThreadHandle) != DefaultThreadHandle) { }

        if (NumItems >= Capacity)
            Grow();

        ListPtr[NumItems++] = value;
        CurrentThread = DefaultThreadHandle;
    }

    /// <summary>
    /// Grows the size of the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Grow()
    {
        // This is a rare path.
        // In practice this will hit a few times and never be hit again as list will eventually be large enough.
        
        // So no fancy code here, needs to be something we can copy into ASM.
        // No intrinsics either, we can't determine platform support without making a mess.
        var oldCapacity = Capacity;
        
        var newItems = (nint*)Malloc((nuint)(Capacity * 2 * sizeof(nuint)));
        for (int x = 0; x < oldCapacity; x++)
            newItems[x] = ListPtr[x];

        Free((nuint)ListPtr);
        ListPtr = newItems;
        Capacity *= 2;
    }
    */
}