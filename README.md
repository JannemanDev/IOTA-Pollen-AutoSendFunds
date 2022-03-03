# IOTA-Pollen-AutoSendFunds

All builds can be found in `Releases`.  
For Synology DS720+ use `linux-x64` build. Ignore the warning about missing file.

## Prerequisites

-A working and synced node  
-At least 1 working Cli-Wallet (configurable through it's own `config.json`)  
 Use the one released with the GoShimmer build:  
	https://github.com/iotaledger/goshimmer/releases/

Important to use the  
	Latest Pollen GUI Wallet: https://github.com/iotaledger/pollen-wallet/releases/  
	Latest node and CLI-Wallet: https://github.com/iotaledger/goshimmer/releases/  
	Latest node software for example as Docker image: https://hub.docker.com/r/iotaledger/goshimmer/tags?page=1&ordering=last_updated&name=latest

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
Windows: `IOTA-Pollen-AutoSendFunds.exe [settingsFile]`  
Linux: `IOTA-Pollen-AutoSendFunds [settingsFile]`  

settingsFile is optional, by default `settings.json` from current directory will be used.  
An example `settings.json` is supplied with each build.

At least review/update the settings.json (more info read further down below):  
-CliWalletFullpath  
-UrlWalletReceiveAddresses  
-UrlWalletNode  
-GoShimmerDashboardUrl  

When using the application:  
 Press escape to quit  
 Press space to pause  
 Press B to show balance of wallet  
 Press R to reload addresses.json and config.json  
 
## How to use as a spammer

-Make multiple folders with each a copy of a `cli-wallet`. Verify that is running correctly.   
-Run `IOTA-Pollen-AutoSendFunds.exe` from each directory where you use the following `settings.json` where X is wallet number, be sure to update the `GoShimmerDashboardUrl`:

```json
{
  "WalletName": "Wallet X",
  "PublishReceiveAddress": true,
  "PublishWebApiUrlOfNodeTakenFromWallet": true,
  "CliWalletFullpath": "c:\\temp\\iota\\wallet X\\cli-wallet.exe",
  "AccessManaId": "",
  "ConsensusManaId": "",
  "UrlWalletReceiveAddresses": "c:\\temp\\iota\\addresses.json",
  "UrlWalletNode": "c:\\temp\\iota\\nodes.json",
  "VerifyIfReceiveAddressesExist": false,
  "GoShimmerDashboardUrl": "http://node.url:8081",
  "MinAmountToSend": 1,
  "MaxAmountToSend": 40,
  "TokensToSent": [
    "IOTA",
    "Janneman2",
    "XV2"
  ],
  "StopWhenNoBalanceWithCreditIsAvailable": false,
  "WaitingTimeInSecondsBetweenTransactions": 0,
  "WaitForPreviousTransactionToFinish": "true",
  "ShowOutputCliWallet": false,
  "PickRandomDestinationAddress": true,
  "MaxWaitingTimeInSecondsForRequestingFunds": 90
}
```

## Settings - settings.json

### CliWalletFullpath

Full path to a cli-wallet. Be sure to use the correct version. See Prerequisites.
Double check if cli-wallet runs correctly.
You can only run one instance of IOTA-Pollen-AutoSendFunds for each cli-wallet (this is auto-checked at runtime).

### AccessManaId

Optional, when left empty it uses your (full) identityID taken by using the `WebAPI` setting from the wallet `config.json`. You can find it yourself by going to http://node.url:8080/info

### ConsensusManaId

Optional, when left empty it uses your (full) identityID taken by using the `WebAPI` setting from the wallet `config.json`. You can find it yourself by going to http://node.url:8080/info

### UrlWalletReceiveAddresses

Two options:
* You can use a fullpath to a local .json file. This file will be auto populated if you set `PublishReceiveAddress` from `settings.json` when you run `IOTA-Pollen-AutoSendFunds.exe`. You can use an example file which is provided in folder `Resources` which already contains dozens of addresses taken from Discord #goshimmer channel.
* (work in progress) Use an url which points to a .json file. For example the central AddressBook webservice where receiving wallet addresses are stored from other users located at: ...

### VerifyIfReceiveAddressesExist

If set to true all receive addresses are checked if available and valid

### GoShimmerDashboardUrl

Points to a node's dashboard, by default this is http://node.url:8081. If empty the node url will be used from the wallet `config.json` with port 8081.
The dashboard is used to check if receive addresses exist and are valid.

### MinAmountToSend and MaxAmountToSend

Minimal and maximal amount to be sent. A random value between these two will be picked. Available balance will be taken into account.

### TokensToSent

Here you can set which tokens will be available to sent. From this set will be randomly chosen.

### StopWhenNoBalanceWithCreditIsAvailable

When there is not any positive balance available left the program will stop when set to true.

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

Maximum time to wait in seconds for requesting funds to complete

## Roadmap / to do / work in progress

AutoSendFunds currently only use a local .json file to store, load and select random wallet addresses from.  

Work in progress:  
-use a local .json file to store, load and select a node to connect to  
-a website/API where users can share their wallet address and/or node ip/port. AutoSendFunds users can choose to load random destination wallet and node info from this website/API in `settings.json`.  
