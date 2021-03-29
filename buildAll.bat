cd /d "C:\MyData\Persoonlijk\IOTA\source\IOTA-Pollen-AutoSendFunds"

dotnet publish source\IOTA-Pollen-AutoSendFunds\IOTA-Pollen-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\linux-arm.pubxml
dotnet publish source\IOTA-Pollen-AutoSendFunds\IOTA-Pollen-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\Linux-x64.pubxml
dotnet publish source\IOTA-Pollen-AutoSendFunds\IOTA-Pollen-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\osx-x64.pubxml
dotnet publish source\IOTA-Pollen-AutoSendFunds\IOTA-Pollen-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\win-x64.pubxml
dotnet publish source\IOTA-Pollen-AutoSendFunds\IOTA-Pollen-AutoSendFunds.csproj /p:PublishProfile=source\Properties\PublishProfiles\win-x86.pubxml
