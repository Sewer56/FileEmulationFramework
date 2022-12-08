# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/reloaded.universal.fileemulationframework.arc/*" -Force -Recurse
dotnet publish "./ARC.Stream.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/reloaded.universal.fileemulationframework.arc" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location