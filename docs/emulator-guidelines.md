!!! info

    Describes the rules, and the two possible `'types'`/ways of implementing emulators.  

## Type A (Stream Based)

!!! info

    One way to produce emulators is through the use of 'streaming'. That is, keeping the minimal amount of information on how to construct the file in memory, while building the rest of the file on the fly as the application requests it.  

In the case of archives, usually applications will read the header of the file, store it, and use that metadata to make further reads from the file.  You should replicate this sort of workflow.

In this case of archives, you would usually produce the whole header of your new virtual archive and store it in memory.  Then as the application creates requests to read the data of the files inside, automatically load said data using either the original archive file, or using new files from the filesystem. 

!!! tip

    Always implement emulators using this technique, if possible. It may be a bit harder, but is far more optimal approach performance wise.

## Type B (Non-stream Based)

!!! info

    In some cases, the stream approach might not make sense; for example in cases where parts of old files might not be directly reusable, or simply hard to reuse.  

    One example of this kind of situation is with combining text/code/scripts. 

In this case, you should simply merge the files manually, to produce a new standalone file, just like you would with a regular program.  

If dealing with small files, it is recommended to write the final file to a `MemoryManagerStream`, and use that stream to fulfill read requests; as reading small files from disk is slow. 

For big files (>100MB) or where the total expected sum of the files is big (2GB+), consider writing them to disk, and fulfilling the read requests from disk.

## Implementation Rules

!!! danger

    Please do not implement hacks for things such as `hotswapping` files at runtime by serving different data on future loads; or writing to buffers passed by the application. Not only are these hard to debug but emulators should be as application agnostic as possible. 

This framework prioritises performance and compatibility first.  

### Always Stream if Possible

Implement your emulator as a `Type A` whenever possible.  

While `Type B` may be easier, since you can potentially simply use existing libraries with `MemoryManagerStream`, it will have a noticeable impact on first load time; and the increased memory usage may lead to increased swapping to/from pagefile. The pagefile also has limits.

### Memory Usage

!!! warning

    Use memory mapped files small files only. It is suggested to write bigger files (>100MB) out to disk directly.

If using using `Type B` emulation, use memory mapped files (`MemoryManager` & `MemoryManagerStream`) when possible. Failure to do so risks virtual address space starvation in 32-bit processes.  

When using memory mapped files, only sections that are currently mapped/viewed use up the address space, in the case of `MemoryManager`, this means only `AllocationGranularity` is used.  

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

### Data Access Patterns

!!! tip

    Assume data can be accessed in any order, and reads may begin from any offset and/or length.  