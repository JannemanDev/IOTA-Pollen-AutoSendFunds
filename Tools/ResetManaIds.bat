cd c:\temp\iota\newwallet1
powershell -Command "(Get-Content settings.json) -replace '.*AccessManaId.*', '  \"AccessManaId\": \"\",' | Set-Content -Path settings.json"
powershell -Command "(Get-Content settings.json) -replace '.*ConsensusManaId.*', '  \"ConsensusManaId\": \"\",' | Set-Content -Path settings.json"

cd c:\temp\iota\newwallet2
powershell -Command "(Get-Content settings.json) -replace '.*AccessManaId.*', '  \"AccessManaId\": \"\",' | Set-Content -Path settings.json"
powershell -Command "(Get-Content settings.json) -replace '.*ConsensusManaId.*', '  \"ConsensusManaId\": \"\",' | Set-Content -Path settings.json"

cd c:\temp\iota\newwallet3
powershell -Command "(Get-Content settings.json) -replace '.*AccessManaId.*', '  \"AccessManaId\": \"\",' | Set-Content -Path settings.json"
powershell -Command "(Get-Content settings.json) -replace '.*ConsensusManaId.*', '  \"ConsensusManaId\": \"\",' | Set-Content -Path settings.json"

cd c:\temp\iota\newwallet4
powershell -Command "(Get-Content settings.json) -replace '.*AccessManaId.*', '  \"AccessManaId\": \"\",' | Set-Content -Path settings.json"
powershell -Command "(Get-Content settings.json) -replace '.*ConsensusManaId.*', '  \"ConsensusManaId\": \"\",' | Set-Content -Path settings.json"

cd c:\temp\iota
