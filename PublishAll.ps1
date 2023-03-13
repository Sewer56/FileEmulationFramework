# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "Emulator/ARC.Stream.Emulator/ARC.Stream.Emulator.csproj" `
              -PackageName "ARC.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/ARC" `
			  -ReadmePath "docs/emulators/arc.md" `
			  -ChangelogPath "Emulator/ARC.Stream.Emulator/CHANGELOG.MD" `
			  
./Publish.ps1 -ProjectPath "Emulator/PAK.Stream.Emulator/PAK.Stream.Emulatorr.csproj" `
              -PackageName "PAK.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/PAK" `
			  -ReadmePath "docs/emulators/pak.md" `
			  -ChangelogPath "Emulator/PAK.Stream.Emulator/CHANGELOG.MD" `

./Publish.ps1 -ProjectPath "Emulator/AFS.Stream.Emulator/AFS.Stream.Emulator.csproj" `
              -PackageName "AFS.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/AFS" `
			  -ReadmePath "docs/emulators/afs.md" `
			  -ChangelogPath "Emulator/AFS.Stream.Emulator/CHANGELOG.MD" `
			  
./Publish.ps1 -ProjectPath "Emulator/AWB.Stream.Emulator/AWB.Stream.Emulator.csproj" `
              -PackageName "AWB.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/AWB" `
			  -ReadmePath "docs/emulators/awb.md" `
			  -ChangelogPath "Emulator/AWB.Stream.Emulator/CHANGELOG.MD" `

./Publish.ps1 -ProjectPath "Emulator/ONE.Heroes.Stream.Emulator/ONE.Heroes.Stream.Emulator.csproj" `
              -PackageName "ONE.Heroes.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/ONE" `
			  -ReadmePath "docs/emulators/one-heroes.md" `
			  -ChangelogPath "Emulator/ONE.Heroes.Stream.Emulator/CHANGELOG.MD" `

./Publish.ps1 -ProjectPath "FileEmulationFramework/FileEmulationFramework.csproj" `
              -PackageName "FileEmulationFramework" `
              -PublishOutputDir "Publish/ToUpload/Framework" `
			  -ReadmePath "docs/index.md" `
			  -ChangelogPath "FileEmulationFramework/CHANGELOG.MD" `

Pop-Location