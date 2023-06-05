# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/BF.File.Emulator/*" -Force -Recurse
dotnet publish "./BF.File.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/BF.File.Emulator" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location