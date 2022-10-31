!!! info

    AWB is a general purpose data container from CRI Middleware.  
    It's the successor to [AFS](./afs.md) and uses the header AFS2.  
    Code for this emulator lives inside main project's GitHub repository.  

## Supported Applications

This emulator should support every application out there.  

It has been tested with the following:  
- Bayonetta (PC)  

## Example Usage

A. Add a dependency on this mod in your mod configuration. (via `Edit Mod` menu dependencies section, or in `ModConfig.json` directly)

```json
"ModDependencies": ["reloaded.universal.fileemulationframework.awb"]
```

B. Add a folder called `FEmulator/AWB` in your mod folder.  
C. Make folders corresponding to AWB Archive names, e.g. `BGM000.AWB`.  

Files inside AWB Archives are accessed by index, i.e. order in the archive: 0, 1, 2, 3 etc.  

Inside each folder make files, with names corresponding to the file's index.  

### Example(s)

To replace a file in an archive named `BGM000.AWB`...

Adding `FEmulator/AWB/BGM000.AWB/0.adx` to your mod would replace the 0th item in the original AWB Archive.

Adding `FEmulator/AWB/BGM000.AWB/32.aix` to your mod would replace the 32th item in the original AWB Archive.

![example](../images/afs/afs_example.png)

File names can contain other text, but must start with a number corresponding to the index.  

!!! info 

    A common misconception is that AWB archives can only be used to store audio. This is in fact wrong. AWB archives can store any kind of data, it's just that using AWB for audio was very popular.

## Limitations (AFS)

The following limitations are known to exist in AFS, and may still apply in AWB, they have been untested.

!!! info 

    For audio playback, you can usually place ADX/AHX/AIX files interchangeably. e.g. You can place a `32.adx` file even if the original AWB archive has an AIX/AHX file inside in that slot. 

!!! info 

    If dealing with AWB audio; you might need to make sure your new files have the same channel count as the originals.   