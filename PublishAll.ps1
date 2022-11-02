# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "Emulator/AFS.Stream.Emulator/AFS.Stream.Emulator.csproj" `
              -PackageName "AFS.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/AFS" `
              -MakeDelta false -UseGitHubDelta false `
			  -ReadmePath "docs/emulators/afs.md" `
			  -ChangelogPath "Emulator/AFS.Stream.Emulator/CHANGELOG.MD" `
              -GitHubUserName Sewer56 -GitHubRepoName Sewer56 -GitHubInheritVersionFromTag false
			  
./Publish.ps1 -ProjectPath "Emulator/AWB.Stream.Emulator/AWB.Stream.Emulator.csproj" `
              -PackageName "AWB.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/AWB" `
              -MakeDelta false -UseGitHubDelta false `
			  -ReadmePath "docs/emulators/awb.md" `
			  -ChangelogPath "Emulator/AWB.Stream.Emulator/CHANGELOG.MD" `
              -GitHubUserName Sewer56 -GitHubRepoName Sewer56 -GitHubInheritVersionFromTag false

./Publish.ps1 -ProjectPath "Emulator/ONE.Heroes.Stream.Emulator/ONE.Heroes.Stream.Emulator.csproj" `
              -PackageName "ONE.Heroes.Stream.Emulator" `
              -PublishOutputDir "Publish/ToUpload/ONE" `
              -MakeDelta false -UseGitHubDelta false `
			  -ReadmePath "docs/emulators/one-heroes.md" `
			  -ChangelogPath "Emulator/ONE.Heroes.Stream.Emulator/CHANGELOG.MD" `
              -GitHubUserName Sewer56 -GitHubRepoName Sewer56 -GitHubInheritVersionFromTag false

./Publish.ps1 -ProjectPath "FileEmulationFramework/FileEmulationFramework.csproj" `
              -PackageName "FileEmulationFramework" `
              -PublishOutputDir "Publish/ToUpload/Framework" `
              -MakeDelta false -UseGitHubDelta false `
			  -ReadmePath "docs/index.md" `
			  -ChangelogPath "FileEmulationFramework/CHANGELOG.MD" `
              -GitHubUserName Sewer56 -GitHubRepoName Sewer56 -GitHubInheritVersionFromTag false

Pop-Location