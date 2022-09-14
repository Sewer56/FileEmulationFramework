<div align="center">
	<h1>Reloaded-II File Emulation Framework</h1>
	<img src="./docs/images/icon.png" Width=200 /><br/>
	<strong>🎈 Let's screw with binaries 🎈</strong>
    <p>A framework for creating virtual files at runtime.</p>
</div>

# The Website

[***Click here to visit the full documentation.***](https://sewer56.dev/FileEmulationFramework/)

## About The Framework

The file emulation framework is a framework for intercepting Windows API calls related to the reading of files from disk; in order to trick games into loading files that don't really exist.  

It builds on top of previous experiments with Reloaded, namely [AFS Redirector](https://github.com/Sewer56/AfsFsRedir.ReloadedII), [ONE Redirector](https://github.com/Sewer56/Heroes.Utils.OneRedirector.ReloadedII) and [Persona 4 Golden PC modloader](https://github.com/tge-was-taken/p4gpc.modloader).  


## A User Friendly Example

Replacing files inside big archives without creating new ones.  

![](./docs/images/afs/afs_example.png)

![](./docs/images/afs/afs_original_file.png)

In this case, the following files would replace the 7th, 8th, 9th and 10th file in the `SH_VOICE_E.afs` archive.  

## How it Works.

By intercepting API calls used to open files, get their properties and read from them, we can essentially create files 'on the fly'; allowing us to perform various forms of post processing such as merging archives in a way that requires zero knowledge of the application running under the hood.

In practice this is extremely effective, the original [AFS Redirector](https://github.com/Sewer56/AfsFsRedir.ReloadedII) is known for being able to work with 10+ games, including those behind emulators.

Projects using this framework are referred to as 'emulators' hence the name `File Emulation Framework`; that name is derived from the original projects which simulated nonexistent archive files.

## How to Contribute (Wiki)

- [Contributing to the Wiki: Online](./docs/guides/contributing-online.md)
- [Contributing to the Wiki: Locally](./docs/guides/contributing-locally.md)

## Credits, Attributions

- Header icon created by <a href="https://www.flaticon.com/free-icons/settings" title="settings icons">Freepik - Flaticon</a>

