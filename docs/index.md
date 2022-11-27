<div align="center">
	<h1>Reloaded-II File Emulation Framework</h1>
	<img src="./images/icon.png" Width=200 /><br/>
	<strong>🎈 Let's screw with binaries 🎈</strong>
    <p>A framework for creating virtual files at runtime.</p>
</div>

## About The Framework

The file emulation framework is a framework for intercepting Windows API calls related to the reading of files from disk; in order to trick games into loading files that don't really exist.  

It builds on top of previous experiments with Reloaded, namely [AFS Redirector](https://github.com/Sewer56/AfsFsRedir.ReloadedII), [ONE Redirector](https://github.com/Sewer56/Heroes.Utils.OneRedirector.ReloadedII) and [Persona 4 Golden PC modloader](https://github.com/tge-was-taken/p4gpc.modloader).  

## A User Friendly Example

Replacing files inside big archives without creating new ones.  

![](./images/afs/afs_example.png)

![](./images/afs/afs_original_file.png)

In this case, the following files would replace the 7th, 8th, 9th and 10th file in the `SH_VOICE_E.afs` archive.  

## How It Works

By intercepting API calls used to open files, get their properties and read from them, we can essentially create files 'on the fly'; allowing us to perform various forms of post processing such as merging archives in a way that requires zero knowledge of the application running under the hood.

In practice this is extremely effective, the original [AFS Redirector](https://github.com/Sewer56/AfsFsRedir.ReloadedII) is known for being able to work with 10+ games, including those behind emulators.

Projects using this framework are referred to as 'emulators' hence the name `File Emulation Framework`; that name is derived from the original projects which simulated nonexistent archive files.

## Performance Impact

This one varies with a lot of factors, such as number of emulators used, the emulators themselves, amount of data emulated, etc.

In most realistic use cases, the emulators usually have negligible performance impact that is completely invisible to the end user. 

Usually first access to a file may be delayed for a small amount of time (`1-3ms on existing emulators at time of writing`) but this should usually be invisible to the end user. Penalty in speed of access to data of emulated files is negligible in practice. 

The biggest penalty tends to be from reading multiple files instead of just one; as the benefits of purely sequential reads may no longer apply.

## How to Contribute (Wiki)

- [Contributing to the Wiki: Online](./guides/contributing-online.md)
- [Contributing to the Wiki: Locally](./guides/contributing-locally.md)

## Credits, Attributions

- Header icon created by <a href="https://www.flaticon.com/free-icons/settings" title="settings icons">Freepik - Flaticon</a>