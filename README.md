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
Windows: IOTA-Pollen-AutoSendFunds.exe  
Linux: IOTA-Pollen-AutoSendFunds  

When using the application:  
 Press escape to quit  
 Press space to pause
 Press B to show balance of wallet
 
## Settings - settings.json

### UrlWalletReceiveAddresses

Here you can find receiving address lists for different versions.  
https://github.com/JanOonk/IOTA-Pollen-AutoSendFunds/tree/main/Receiving%20addresses

In the IOTA discord server in the #goshimmer-discussion channel you can drop off your receive address.

But you can use any url where receive addresses are stored as plain text.
This needs to be 1 wallet receive address per line optionally prefixed with the name of the owner (may include spaces) and separated with tab(s).

### AccessManaId

Optional, by default it uses your (full) identityID which can be found on http://node.url:8080/info

### ConsensusManaId

Optional, by default it uses your (full) identityID which can be found on http://node.url:8080/info

### CliWalletFullpath

Full path to the cli-wallet. Be sure to use the correct version. See Prerequisites.
Double check if it runs correctly and has a balance.

### UrlWalletReceiveAddresses

You can use an url (http/https) 
	"UrlWalletReceiveAddresses" : "https://raw.githubusercontent.com/JanOonk/IOTA-Pollen-AutoSendFunds/main/Pollen-0.5.1fix-Receiving-Addresses.txt",

or a local file with full path.
For example on Windows:
	"UrlWalletReceiveAddresses" : "c:\\Temp\\Pollen-0.5.2-Receiving-Addresses-LocalDev.txt"

### MinAmountToSend and MaxAmountToSend

Minimal and maximal amount to be sent. A random value between these two will be picked. Available balance will be taken into account.

### TokensToSent

Here you can set which tokens will available to sent. From this set will be randomly chosen.

### TimeoutInSecondsWhenNoBalanceWithCreditIsAvailable

When there is not any positive balance availble left, there will be a waiting period before trying again. 
This period can be used to wait for incoming funds.
Only applicable when StopWhenNoBalanceWithCreditIsAvailable is false.

### StopWhenNoBalanceWithCreditIsAvailable

When there is not any positive balance availble left the program will stop when set to true.

### TimeoutInSecondsBetweenTransactions

Time to wait in seconds between transactions.

### ShowOutputCliWallet

Show output from cli-wallet

### PickRandomDestinationAddress

If true randomly a destinationaddress, which is not owned by you, will be picked from UrlWalletReceiveAddresses.
If false this will be done sequentially.

## Roadmap / to do

-When funds are low automatically request funds from the faucet
