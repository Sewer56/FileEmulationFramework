# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/Reloaded.Universal.FileEmulationFramework/*" -Force -Recurse
dotnet publish "./FileEmulationFramework.csproj" -c Release -o "$env:RELOADEDIIMODS/Reloaded.Universal.FileEmulationFramework" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location