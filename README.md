# IOTA-Pollen-AutoSendFunds

All builds can be found in \bin folder. 
For Synology DS720+ use linux-x64 build. Ignore the warning about missing file.

## Prerequisites

-A working and synced node  
-A working CLI-Wallet (configurable through it's own config.json)  
 Use the one released with the GoShimmer build:  
	https://github.com/iotaledger/goshimmer/releases/

Important to use the  
	Latest Pollen GUI Wallet: https://github.com/iotaledger/pollen-wallet/releases/  
	Latest node and CLI-Wallet: https://github.com/iotaledger/goshimmer/releases/  
	Latest node software as Docker image: https://hub.docker.com/r/iotaledger/goshimmer/tags?page=1&ordering=last_updated&name=latest

## Dependencies
    
If you want to build it yourself you can use the buildAll.bat (Windows).  
Developed and test with Visual Studio 2019 v16.8.6 with .NET Core 5  

NuGet packages used:  
    CliWrap Version="3.3.1" - https://github.com/Tyrrrz/CliWrap  
    Newtonsoft.Json Version="12.0.3" - https://www.newtonsoft.com/json  
    RestSharp Version="106.11.7" - https://restsharp.dev/  
    SimpleBase Version="3.0.2" - https://www.nuget.org/packages/SimpleBase  

## How to use

See \bin\Release\net5.0\publish  
Run for your platform for example:  
Windows: IOTA-Pollen-AutoSendFunds.exe [settingsFile]  
Linux: IOTA-Pollen-AutoSendFunds [settingsFile]  

settingsFile is optional, by default `settings.json` from current directory will be used
An example `settings.json` is supplied

When using the application:  
 Press escape to quit  
 Press space to pause
 Press B to show balance of wallet
 
## Settings - settings.json

### CliWalletFullpath

Full path to the cli-wallet. Be sure to use the correct version. See Prerequisites.
Double check if it runs correctly and has a balance.
You can only run one instance of IOTA-Pollen-AutoSendFunds for each cli-wallet!

### AccessManaId

Optional, by default it uses your (full) identityID. For this the WebAPI setting is used from the wallet `config.json`, this points to http://node.url:8080/info

### ConsensusManaId

Optional, by default it uses your (full) identityID. For this the WebAPI setting is used from the wallet `config.json`, this points to http://node.url:8080/info

### UrlWalletReceiveAddresses

Two options:
* You can use a local .json file. An example file is provided in folder `Receiving addresses`
* Use an url which points to a .json file. For example the central AddressBook webservice where receiving wallet addresses are stored from other users located at: ...

### VerifyIfReceiveAddressesExist

If set to true all receive addresses are checked if available and valid

### GoShimmerDashboardUrl

Points to a node's dashboard, by default this is http://node.url:8081. If empty the node url will be used from the wallet `config.json` with port 8081.
The dashboard is used to check if receive addresses are available.

### MinAmountToSend and MaxAmountToSend

Minimal and maximal amount to be sent. A random value between these two will be picked. Available balance will be taken into account.

### TokensToSent

Here you can set which tokens will be available to sent. From this set will be randomly chosen.

### StopWhenNoBalanceWithCreditIsAvailable

When there is not any positive balance availble left the program will stop when set to true.

### WaitingTimeInSecondsBetweenTransactions

Time to wait in seconds between transactions.

### WaitForPreviousTransactionToFinish

Not used currently

### ShowOutputCliWallet

Show output from cli-wallet

### PickRandomDestinationAddress

If true randomly a destinationaddress, which is not owned by you, will be picked from UrlWalletReceiveAddresses.
If false this will be done sequentially.

### MaxWaitingTimeInSecondsForRequestingFunds

Time to wait in seconds for requesting funds to complete

## Roadmap / to do

-When funds are low automatically request funds from the faucet
