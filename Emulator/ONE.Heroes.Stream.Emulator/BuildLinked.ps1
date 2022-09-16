# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/ONE.Heroes.Stream.Emulator/*" -Force -Recurse
dotnet publish "./ONE.Heroes.Stream.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/ONE.Heroes.Stream.Emulator" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location