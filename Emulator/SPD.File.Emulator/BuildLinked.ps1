# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/SPD.File.Emulator/*" -Force -Recurse
dotnet publish "./SPD.File.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/SPD.File.Emulator" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location