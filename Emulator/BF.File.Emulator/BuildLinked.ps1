# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/reloaded.universal.fileemulationframework.bf/*" -Force -Recurse
dotnet publish "./BF.File.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/reloaded.universal.fileemulationframework.bf" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location