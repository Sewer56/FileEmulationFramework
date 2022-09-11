## High Performance Logging

!!! tip

    The `Logger` class in `FileEmulationFramework.Lib` provides an abstraction over Reloaded's `ILogger` that allows you to conditionally make log messages without the need for memory allocation.  

```csharp
_log = new Logger(_logger, _configuration.LogLevel);
```

Methods such as `_log.Info` will only create the string and log it if that specific log level is enabled; provided you use the various overloads that accept generics e.g. `Info<T1>(string format, T1 item1)`.

Avoid the overloads with single message parameter unless you require custom formatting; in which case you should add manual guards via `if (_log.IsEnabled(level))`.

## File Slices

!!! info

    The `FileSlice` class can be used to provide an abstraction that allows you to read a region of a given file.  

When building Stream based emulators, you will often provide a mixture of the original data and new data. This class will allow you to more easily fetch the original data when needed.

### Merging File Slices

!!! info

    Slices of the same file that touch each other (e.g. `0-4095` + `4096-65536`) can be merged into singular, larger slices.  

    This is sometimes possible when working with archives containing file data whereby multiple files are laid out side by side.

!!! tip

    Merging can help improve performance of resolving `Read` requests (i.e. `IEmulator.ReadData`). Specifically the performance of [StreamMixer](#streammixer)

Try using the `FileSlice.TryMerge` API.

## File Slice Stream

!!! info

    The `FileSliceStream` classes provide an abstraction that wrap `FileSlice`(s).  
    Currently two implementations exist, `FileSliceStreamFs` and `FileSliceStreamW32`.  

!!! tip

    `FileSliceStreamFs` is backed by FileStream. Use this class if application makes many reads with small amount of data (e.g. <= 256 byte reads.)

!!! tip

    `FileSliceStreamW32` is backed by Windows API. Use this class if reads above 4096 bytes are expected.

Should be simple enough.

## StreamMixer

## Fast Directory Searcher

!!! tip

    The `WindowsDirectorySearcher` class can be used for extremely fast searching of files on the filesystem. 

This implementation is around 3.5x faster than the built in .NET one at the time of writing; using the innermost `NtQueryDirectoryFile` API for fetching files.

It's forked from the implementation in `Reloaded.Mod.Loader.IO`; with Multithreading removed since it wouldn't be helpful with our too small data/file sets.