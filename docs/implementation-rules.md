## Implementation Rules

!!! danger

    Please do not implement hacks for things such as `hotswapping` files at runtime by serving different data on future loads; or writing to buffers passed by the application. Not only are these hard to debug but emulators should be as application agnostic as possible.  

    APIs to enable those features can be provided for other mods to use (e.g. via Dependency Injection), but must not be enabled by default.  

This framework prioritises performance and compatibility first.  

### Always Stream if Possible

Implement your emulator as a `Type A` whenever possible.  

While `Type B` may be easier, since you can potentially simply use existing libraries with `MemoryManagerStream`, it will have a noticeable impact on first load time; and the increased memory usage may lead to increased swapping to/from pagefile. The pagefile also has limits.

### Memory Usage

!!! warning

    Use memory mapped files for small files only. It is suggested to write bigger files (>100MB) out to disk directly.

If using using `Type B` emulation, use memory mapped files (`MemoryManager` & `MemoryManagerStream`) when possible. Failure to do so risks virtual address space starvation in 32-bit processes.  

When using memory mapped files, only sections that are currently mapped/viewed use up the address space, in the case of `MemoryManager`, this means only `AllocationGranularity` is used.  

!!! warning

    For files smaller than `AllocationGranularity` use a `MemoryStream` instead to avoid wasting address space.

### Use Lazy Loading & Immutability

Implementations should only produce/initialize emulated files when they are first requested by the application; i.e. when a handle is opened.  

Once produced, the file emulator should always serve the same file on subsequent requests/handle openings. i.e. generated files persist for application lifetime.

### Always Read All Requested Bytes

!!! info

    A common programmer error is to issue a `Read()` command on a file stream and assume that all bytes requested will be given back.

This is not often the case and even I have been guilty of this mistake for a very long time. If possible, ***DO NOT*** return less than the number of bytes requested (when possible) in order to shield against buggy software implementations.  

While this may sound more complicated than it should for e.g. archives, it really should not be. If you have some code for an archive emulator's `ReadData` that looks something like:

```csharp
// If getting header in Type-A emulator
if (isHeaderRead)
{
    // We are reading the file header, let's give the program the false header.
    var fakeHeaderSpan = new Span<byte>(afsFile.HeaderPtr, afsFile.Header.Length);
    var endOfHeader = offset + length;
    if (endOfHeader > fakeHeaderSpan.Length)
        length -= (uint)(endOfHeader - fakeHeaderSpan.Length);

    var slice = fakeHeaderSpan.Slice((int)offset, (int)length);
    slice.CopyTo(bufferSpan);

    numReadBytes = slice.Length;
    return true;
}

// Else we are reading a file, let's pass a new file to the buffer.
if (afsFile.TryFindFile((int)offset, (int)length, out var virtualFile))
{
    numReadBytes = virtualFile.GetData(bufferSpan);
    return true;
}
```

Then you can just invoke this function multiple times until the requested amount of bytes have been filled.

!!! tip

    A recommended way of building stream (Type A) based emulators emulators is [MultiStream](./implementation-utilities.md#multi-stream). With MultiStream you can avoid this issue entirely, and usually implement `ReadData` in ~5 lines. 

### Hooks Always Enabled

Don't deactivate your hooks at any point. All hooks should always be enabled to allow for recursive use of the emulators.

### Data Access Patterns

!!! tip

    Assume data can be accessed in any order, and reads may begin from any offset and/or length.  