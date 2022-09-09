# File Emulation Framework


## Type A (Stream Based)

!!! info

    Some formats are 'streamed', or in the case of archives, have individual files extracted from them.  
    Usually applications will read the header of the file, store it, and use that metadata to make further reads from the file.  

    In this case, we produce the whole header, and feed it to the application.  Then manually resolve the requests for contents of the archives.


## Type B (Non-stream Based)

!!! info

    Some applications might read the whole file in at once in the case of smaller files.  
    This means we have to produce the whole file at once, and give it to the application when it is requested.  


## Implementation Rules

This framework prioritises performance and compatibility first.  

You should not implement hacks for e.g. hotswapping files at runtime by serving different data on subsequent loads, or writing to buffers passed by the application; because those hacks can be application/implementation specific and hard to debug.  

### Memory Usage

Use memory mapped files (and/or `MemoryManager`) when possible. Failure to do so risks virtual address space starvation in 32-bit processes.  

When using memory mapped files, sections that are currently mapped/viewed use up the address space, in the case of `MemoryManager`, this means only `AllocationGranularity` used.  

### Prefer Streaming when Possible

Implement your emulator as a `Type A` whenever possible.  

While `Type B` may be easier, since you can potentially simply use existing libraries with `MemoryManagerStream`, it will have a noticeable impact on first load time; and the increased memory usage may lead to increased swapping to/from pagefile.  

### Use Lazy Loading & Immutability

Implementations should only produce/initialize emulated files when they are first requested by the application; i.e. when a handle is opened.  

Once produced, the file emulator should always serve the same file on subsequent requests/handle openings. i.e. generated files persist for application lifetime.