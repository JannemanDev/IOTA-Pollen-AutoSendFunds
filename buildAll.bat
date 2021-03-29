cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"

rem dotnet publish source\IOTA-Pollen-AutoSendFunds\Dashboard-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\linux-arm.pubxml
dotnet publish source\IOTA-Pollen-AutoSendFunds\Dashboard-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\Linux-x64.pubxml
rem dotnet publish source\IOTA-Pollen-AutoSendFunds\Dashboard-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\osx-x64.pubxml
rem dotnet publish source\IOTA-Pollen-AutoSendFunds\Dashboard-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\win-x64.pubxml
rem dotnet publish source\IOTA-Pollen-AutoSendFunds\Dashboard-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\win-x86.pubxml
