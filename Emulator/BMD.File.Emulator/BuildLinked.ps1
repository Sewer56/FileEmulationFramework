# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/Bmd.File.Emulator/*" -Force -Recurse
dotnet publish "./Bmd.File.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/Bmd.File.Emulator" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location