!!! info

    AFS is a general purpose data container from CRI Middleware.  

## Supported Applications

This emulator should support every application out there.  

It has been tested with the following:  
- Sonic Heroes (PC)  
- Silent Hill 3 (PC)  
- Sonic Adventure 2 (PC)  
- Shadow The Hedgehog (Japanese, GameCube, Dolphin Emulator, Running from FileSystem)  

## Example Usage

A. Add a dependency on this mod in your mod configuration. (via `Edit Mod` menu dependencies section, or in `ModConfig.json` directly)

```json
"ModDependencies": ["reloaded.universal.fileemulationframework.afs"]
```

B. Add a folder called `FEmulator/AFS` in your mod folder.  
C. Make folders corresponding to AFS Archive names, e.g. `SH_VOICE_E.AFS`.  

Files inside AFS Archives are accessed by index, i.e. order in the archive: 0, 1, 2, 3 etc.  
Inside each folder make files, with names corresponding to the file's index.  

## Folder Support

This redirector supports 2 folders inside mods:  
- `AfsRedirector` (for support of legacy mod)  
- `FEmulator/AFS` (use this)

### Example(s)

To replace a file in an archive named `EVENT_ADX_E.AFS`...

Adding `FEmulator/AFS/EVENT_ADX_E.AFS/0.adx` to your mod would replace the 0th item in the original AFS Archive.

Adding `FEmulator/AFS/EVENT_ADX_E.AFS/32.aix` to your mod would replace the 32th item in the original AFS Archive.

**Note 1:**
Generally, for audio playback, you can place ADX/AHX/AIX files interchangeably, e.g. You can place a `32.adx` file even if the original AFS archive has an AIX file inside in that slot. 

**Note 2:** A common misconception is that AFS archives can only be used to store audio. This is in fact wrong. AFS archives can store any kind of data, it's just that using AFS for audio was very popular.