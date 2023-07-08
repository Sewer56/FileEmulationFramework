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

If dealing with small files, it is recommended to write the final file to a `MemoryManagerStream`, and use that stream to fulfill read requests; as reading small files from disk is slow. For very small files (<64KiB) use a `MemoryStream` instead. 

For big files (>100MB) or where the total expected sum of the files is big (2GB+), consider writing them to disk, and fulfilling the read requests from disk.