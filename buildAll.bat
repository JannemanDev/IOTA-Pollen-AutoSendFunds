set version=0.1

rem Root solution folder
cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"


rem Delete Builds folder
del /f /q /s Builds\*.* > nul
rmdir /q /s Builds
mkdir Builds


rem Delete bin folder of AutoSendFunds
del /f /q /s source\AutoSendFunds\bin\*.* > nul
rmdir /q /s source\AutoSendFunds\bin

dotnet publish source\AutoSendFunds\AutoSendFunds.csproj /p:PublishProfile=source\AutoSendFunds\Properties\PublishProfiles\linux-arm.pubxml --configuration Release
dotnet publish source\AutoSendFunds\AutoSendFunds.csproj /p:PublishProfile=source\AutoSendFunds\Properties\PublishProfiles\linux-x64.pubxml --configuration Release
dotnet publish source\AutoSendFunds\AutoSendFunds.csproj /p:PublishProfile=source\AutoSendFunds\Properties\PublishProfiles\osx-x64.pubxml --configuration Release
dotnet publish source\AutoSendFunds\AutoSendFunds.csproj /p:PublishProfile=source\AutoSendFunds\Properties\PublishProfiles\win-x64.pubxml --configuration Release
dotnet publish source\AutoSendFunds\AutoSendFunds.csproj /p:PublishProfile=source\AutoSendFunds\Properties\PublishProfiles\win-x86.pubxml --configuration Release


rem Delete bin folder of AddressBookWebService
del /f /q /s source\AddressBookWebService\bin\*.* > nul
rmdir /q /s source\AddressBookWebService\bin

dotnet publish source\AddressBookWebService\AddressBookWebService.csproj /p:PublishProfile=source\AddressBookWebService\Properties\PublishProfiles\linux-arm.pubxml --configuration Release
dotnet publish source\AddressBookWebService\AddressBookWebService.csproj /p:PublishProfile=source\AddressBookWebService\Properties\PublishProfiles\linux-x64.pubxml --configuration Release
dotnet publish source\AddressBookWebService\AddressBookWebService.csproj /p:PublishProfile=source\AddressBookWebService\Properties\PublishProfiles\osx-x64.pubxml --configuration Release
dotnet publish source\AddressBookWebService\AddressBookWebService.csproj /p:PublishProfile=source\AddressBookWebService\Properties\PublishProfiles\win-x64.pubxml --configuration Release
dotnet publish source\AddressBookWebService\AddressBookWebService.csproj /p:PublishProfile=source\AddressBookWebService\Properties\PublishProfiles\win-x86.pubxml --configuration Release


rem Rar all AddressBookWebService builds
cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AddressBookWebService\bin\Release\net5.0\linux-arm\publish
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AddressBookWebService-%version%-linux-arm.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AddressBookWebService\bin\Release\net5.0\linux-x64\publish
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AddressBookWebService-%version%-linux-x64.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AddressBookWebService\bin\Release\net5.0\osx-x64\publish
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AddressBookWebService-%version%-osx-x64.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AddressBookWebService\bin\Release\net5.0\win-x64\publish
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AddressBookWebService-%version%-win-x64.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AddressBookWebService\bin\Release\net5.0\win-x86\publish
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AddressBookWebService-%version%-win-x86.rar *.*


rem Rar all AutoSendFunds builds
cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AutoSendFunds\bin\Release\net5.0\publish\linux-arm
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AutoSendFunds-%version%-linux-arm.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AutoSendFunds\bin\Release\net5.0\publish\linux-x64
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AutoSendFunds-%version%-linux-x64.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AutoSendFunds\bin\Release\net5.0\publish\osx-x64
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AutoSendFunds-%version%-osx-x64.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AutoSendFunds\bin\Release\net5.0\publish\win-x64
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AutoSendFunds-%version%-win-x64.rar *.*

cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
cd source\AutoSendFunds\bin\Release\net5.0\publish\win-x86
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\AutoSendFunds-%version%-win-x86.rar *.*


cd /d "C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds"
