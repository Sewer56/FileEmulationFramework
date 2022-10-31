# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/reloaded.universal.fileemulationframework.awb/*" -Force -Recurse
dotnet publish "./AWB.Stream.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/reloaded.universal.fileemulationframework.awb" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location