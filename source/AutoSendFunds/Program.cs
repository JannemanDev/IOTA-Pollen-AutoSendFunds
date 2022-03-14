using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using IOTA_Pollen_AutoSendFunds.ExtensionMethods;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SharedLib.SettingsModels;
using SharedLib.Models;
using SharedLib.Repositories.Addresses;
using SharedLib.Services;
using SimpleBase;
using SharedLib;

/*
 * "I noticed cli-wallet.exe changed it behaviour when sending funds for example... now you can't send two transactions in a row when balance is still pending."
 * "If you have all your funds on the same output, how do you expect to send the second tx when the first tx spent that output? 
 *  In this case, you need to wait until the first tx confirms, so you have available funds in the wallet.
 *  If however you have let's say 100-100 funds on two outputs, you can send two txs spending these simultaneously, because there is no dependency between the txs."
 *
 * But the actual problem is caused because you have too many UTXOs on the addresses in the wallet.
 * Consolidating funds is currently not supported by the cli wallet on the master branch but it is on develop.
 * However, you should be able to also do this manually by sending transactions to yourself with a smaller balance than your total.
 *
 * Todo: consolidate function send total sum of each token to your own address (after X spent addresses do this) (make it a setting)
 * Todo: check server how many transactions have been sent to a certain destaddress (make this also a serversetting to override/maximise it)
 * See https://discord.com/channels/397872799483428865/603609366452502538/836247284961771630
 * Alive api endpoint for autofunds.ddns.net -> if not then spammer only locally
 *
 * Node UI mk (vergelijkbaar met wallet address)
 *
 * Use a database instead of json file for addresses
 * LogLevel instelbaar maken via settings of serilog settings file?
 *
 * v Show only errors / or also option(?)
 * - AutoUpdateReceiveAddress after x seconds (in settings)
 *
 * Default settings file meeleveren
 *
 * Alternatieve node: http://45.83.107.51:8080/
 *
 * refactor: new Address moet een isspent hebben wat in regel 51 homecontroller niet bekend is
 * walletname (for new address()): if empty generate autoname
 *
 * not in sync error afvangen / rekening mee houden
 *
 * check of alle transacties opgeteld kloppen met balance?
 *
 * output headers wegparsen ? bijv. bij Balance opvragen via dashboard
 * error: weghalen indien geen error!
 *
 * v auto wallet create
 * v auto (re)request funds
 * - wallet webapi node kiezen uit nodes repo
 * - nodes uit nodes.json kunnen disablen/enablen
 * - make an option for: use same receive adress or use unspent adres
 * - save wallet address in api repo
 * v bij wegschrijven json eerst opmaken

 * Netwerkversion en versie bewaren en alles hieraan hangen (log, nodes, ...)
 *      
 * AddressBook:
 *  log endpoint maken -> bewaren in sqlite?
 *  grafieken
 *  1 of meer nodes (per netwerkversie/versie) ivm o.a. onlinecheck
 *
 * bugreport error vs output cli-wallet -> testen met cmdline &2 &1 
 *
 * Checken of addresses wel bestaan: via WebAPI uit cli-wallet's config.json
 * 0-9 sneltoets voor eerste 10 adressen gebruiken om naar te zenden
 * Dit is dashboard app
 * Outputlog incl. trans ID (alleen error laten zien indien error, bovenaan success/failed)
 * L=Looped sending toggle on/off
 * Transaction ID genereren met begin- en eind-timestamp
 * Fire and forget transaction
 * Als trans klaar is wegschrijven naar outputlog (keep retrying)
 * If log changed Transaction done counter refresh (failed en success)
 * If balance update refresh
 *
 * LogViewer app (continue refresh)
 *
 * Addressbook API + MVC CRUD service
 *  List addresses (plain text)
 *  Simpel form om te adden/updaten
 *  Opslaan in JSON structuur
 *  upsert endpoint
 *  vanuit console receive address posten, list ophalen
 *  settings: publish
 *  json ipv txt
 *  logic in service klasse zetten zodat zowel rest als mvc controller deze kan aanroepen
 *  list of addresses
 *  api/mvc server hosten op: fhict, docker filesrv3, ...?
 */

//Todo:
//-console output gaat door elkaar agv async stuff
//-address validation (sometimes its seed or only 1 column etc..)
//-check for correct version icm addresses
//-test requestFunds
//-test filtering on tokenstosent
//-test with colored tokens -> color token is uniek (=address), tokenname hoeft niet uniek te zijn?!
//-after each trans. show stats like total send, average conf.time, ...
//-autocreate minted tokens and add them to the settings: TokensToSent

//-calculate and show total tx, nr of errors, average tx speed in seconds and persist this in a file so you can stop and restart
//  file should include cli-wallet (and autosendfunds?) version

// bij sending eerst sync status checken anders krijg je error: can't issue payload: tangle not synced
//   ./cli-wallet server-status
//   IOTA 2.0 DevNet CLI-Wallet 0.2                                                                                                                                                                              
//   Server ID:  E35sPFNueGQHgQCUFJPsz4mqqmFzD3tHbEho5H4nZTu7
//   Server Synced:  false
//   Server Version:  v0.8.8
//   Delegation Address:  13aUicm8cZPWaPJTuWqu4jENHE2jwY1EyxR9rSFnQVXCK

// when error bij sending dan niet "sending complete... waiting for ..." loggen!
// fetching balance niet telkens loggen (alleen eerste keer en laatste keer). Het vult/vervuilt de log nogal

// be sure to check node version to be same as set in settings.json!!
// bij selecteren van node alleen synced node pakken

// seperate transaction log

// option to get /use wallet receive addresses taken from the explorer

// option to verify nodes when loading
// test when node is offline later on... will it keep running?

// verifyaddress mbv website werkt niet als wallet net gemaakt is of nog geen txs zijn gedaan!
// sommige API van nodes hebben login/ww nodig (gebruiken reverse proxy) -> node lijst uitbreiden hiermee

namespace IOTA_Pollen_AutoSendFunds
{
    class Program
    {
        public static Settings settings;

        static string lockFile = "";

        static string settingsFile;
        static string cliWalletConfigFolder;
        static CliWallet cliWallet;
        static List<Node> nodes;
        static Random random = new Random();
        static int nodeIndex;
        static int receiveAddressIndex;
        static List<Address> receiveAddresses;
        static AddressService addressService;
        static CliWalletConfig cliWalletConfig;

        static async Task Main(string[] args)
        {
            //Create default minimal logger until settings are loaded
            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose() //send all events to sinks
             .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
             .CreateLogger();

            Log.Logger.Information(("Dashboard - IOTA-Pollen-AutoSendFunds v0.13 - 13 March 2022\n"));
            Log.Logger.Information(" Escape to quit");
            Log.Logger.Information(" Space to pause\n");

            settingsFile = ParseArguments(args);

            if (!File.Exists(settingsFile))
            {
                Log.Logger.Fatal($"Settingsfile {settingsFile} not found!"); ;
                CleanExit(1);
            }

            //settingsFile = "c:\\temp\\iota\\newwallet3\\settings.json"; //override for testing purposes

            await Init(true);

            bool waitForAnyPositiveBalance = false;
            while (true)
            {
                var balancePicked = new { Color = "", Value = 0 }; //init to make compiler happy
                bool resultRequestFunds = true;
                do
                {
                    await cliWallet.UpdateBalances();

                    //pick random token
                    var filtered = cliWallet.Balances
                        .Where(balance => balance.BalanceStatus == BalanceStatus.Ok)
                        .Where(balance => settings.TokensToSent.Contains(balance.TokenName)) //only consider tokens from the settings
                        .GroupBy(balance => balance.Color)  //groupby Color (which is unique)
                        .Select(group => new
                        { //sum all ok/pend balances for each token
                            Color = group.Key,
                            Value = group.Sum(x => x.BalanceValue)
                        })
                        .Where(colorToken => colorToken.Value >= settings.MinAmountToSend) //only with enough balance
                        .ToList();

                    balancePicked = filtered
                        .RandomElement();

                    if (waitForAnyPositiveBalance && balancePicked != null)
                    {
                        Log.Logger.Information(cliWallet.ToString());
                        waitForAnyPositiveBalance = false;
                    }

                    if (!waitForAnyPositiveBalance) Log.Logger.Information(cliWallet.ToString());

                    if (balancePicked == null)
                    {
                        string tokens = String.Join(", ", settings.TokensToSent);
                        if (!waitForAnyPositiveBalance && balancePicked == null) Log.Logger.Warning($"None of the tokens '{tokens}' have a Ok balance with at least {settings.MinAmountToSend}!");
                        if (settings.StopWhenNoBalanceWithCreditIsAvailable)
                        {
                            CleanExit(0);
                        }
                        //als er geen pending IOTA balance is dan request funds
                        else if (settings.TokensToSent.Contains("IOTA") && !cliWallet.Balances.Any(balance => balance.Color == "IOTA" && balance.BalanceStatus == BalanceStatus.Pending))
                        {
                            resultRequestFunds = await cliWallet.RequestFunds();
                        }
                        else
                        {
                            if (!waitForAnyPositiveBalance) Log.Logger.Information($"Waiting for any of above tokens to arrive!");
                            waitForAnyPositiveBalance = true;
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    else waitForAnyPositiveBalance = false;
                } while (balancePicked == null && (resultRequestFunds));

                if (!resultRequestFunds) CleanExit(1);

                bool sendFundsResult = true;
                if (balancePicked != null)
                {
                    //random amount respecting available balanceValue
                    int min = Math.Min(settings.MinAmountToSend, balancePicked.Value);
                    int max = Math.Min(settings.MaxAmountToSend, balancePicked.Value);
                    int amount = random.Next(min, max);

                    //pick next destination address
                    if (settings.PickRandomDestinationAddress) //random
                        receiveAddressIndex = random.Next(receiveAddresses.Count);
                    else //sequential
                        receiveAddressIndex = (receiveAddressIndex + 1) % receiveAddresses.Count;

                    Address receiveAddress = receiveAddresses[receiveAddressIndex];

                    //determine access- and consensus mana pledging
                    string accessManaId = DetermineToWhichNodeToPledgeTo(settings.AccessManaId, cliWalletConfig);
                    string consensusManaId = DetermineToWhichNodeToPledgeTo(settings.ConsensusManaId, cliWalletConfig);

                    if (settings.NodeSelectionMethod != NodeSelectionMethod.Static)
                        Log.Logger.Information($"Using node {cliWalletConfig.WebAPI} with identity {MiscUtil.GetIdentityId(cliWalletConfig.WebAPI)}");

                    sendFundsResult = await cliWallet.SendFunds(amount, receiveAddress, balancePicked.Color, accessManaId, consensusManaId);

                    if (sendFundsResult)
                    {
                        int remainingTimeBetweenTransactions = settings.WaitingTimeInSecondsBetweenTransactions;
                        while (remainingTimeBetweenTransactions > 0)
                        {
                            if (remainingTimeBetweenTransactions == settings.WaitingTimeInSecondsBetweenTransactions)
                                Log.Logger.Debug($"\nSleeping between transactions:");
                            Log.Logger.Debug($" {remainingTimeBetweenTransactions}");

                            Thread.Sleep(1000);
                            remainingTimeBetweenTransactions--;

                            if (remainingTimeBetweenTransactions == 0) Log.Logger.Information("");
                        }
                    }
                }

                if (!sendFundsResult)
                {
                    Log.Logger.Error("Sending funds failed with an error and/or to return to OK balance took too long.");
                    Log.Logger.Information("Pausing... Press <space> to continue, B = Balance, R = Reload Addresses, Nodes and Config, <escape> to quit!");
                }

                bool pause = !sendFundsResult;
                while (Console.KeyAvailable || (pause))
                {
                    if (Console.KeyAvailable || (pause))
                    {
                        ConsoleKeyInfo cki = Console.ReadKey(true);
                        if (cki.Key == ConsoleKey.Escape) CleanExit(0);
                        if (cki.Key == ConsoleKey.Spacebar)
                        {
                            if (pause) pause = false; //continue
                            else
                            {
                                Log.Logger.Information("Pausing... Press <space> to continue, B = Balance, R = Reload Addresses, Nodes and Config, <escape> to quit!");
                                pause = true;
                            }
                        }

                        if (cki.Key == ConsoleKey.B) //show balance
                        {
                            await cliWallet.UpdateBalances();
                            Log.Logger.Information(cliWallet.ToString());
                        }

                        if (cki.Key == ConsoleKey.R) //reload addresses and config
                        {
                            Log.Logger.Information("Reloading Addresses, Nodes and Config!");
                            await Init(false);
                        }
                    }
                }

                //pick node
                nodeIndex = PickNode(cliWalletConfigFolder, cliWalletConfig, nodes, random, nodeIndex);
            }
        }

        private static async Task Init(bool firstTime)
        {
            try
            {
                settings = MiscUtil.LoadSettings(settingsFile);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                CleanExit(1);
            }
            MiscUtil.WriteSettings(settingsFile, settings);

            InitLogging();

            cliWalletConfigFolder = Path.GetDirectoryName(settings.CliWalletFullpath);

            if (firstTime)
            {
                WriteLockFile(cliWalletConfigFolder);

                //Todo: temporary solution for error: "The SSL connection could not be established, see inner exception."
                //       when using RestSharp.
                //      Postman generates a warning: "Unable to verify the first certificate"
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }
            else
            {
                RemoveLockFile();
                WriteLockFile(cliWalletConfigFolder);
            }

            nodeIndex = 0;
            NodeService nodeService = new NodeService(RepoFactory.CreateNodeRepo(settings.UrlWalletNodes));

            //init cliWallet
            cliWallet = new CliWallet();
            nodes = LoadEnabledNodes(cliWallet, nodeService);

            //pick a node so we can update addresses
            cliWalletConfig = await LoadCliWalletConfig(cliWalletConfigFolder);
            nodeIndex = PickNode(cliWalletConfigFolder, cliWalletConfig, nodes, random, nodeIndex);
            if (settings.NodeSelectionMethod == NodeSelectionMethod.Static)
                Log.Logger.Information($"Using node {cliWalletConfig.WebAPI} with identity {MiscUtil.GetIdentityId(cliWalletConfig.WebAPI)}");

            //first check if wallet exist in folder where cli-wallet executable is in
            if (!CliWallet.Exist(settings.CliWalletFullpath))
            {
                Log.Logger.Warning($"Wallet not found! File does not exist: {CliWallet.DefaultWalletFile(settings.CliWalletFullpath)}");
                //CleanExit(1);
                Log.Logger.Information("Creating new wallet!");
                await CliWallet.Init();
            }

            //now we can update addresses
            await cliWallet.UpdateAddresses();

            addressService = new AddressService(RepoFactory.CreateAddressRepo(settings.UrlWalletReceiveAddresses), settings.GoShimmerDashboardUrl);

            if (settings.PublishReceiveAddress) addressService.Add(cliWallet.ReceiveAddress);
            else addressService.Delete(cliWallet.ReceiveAddress.AddressValue);

            receiveAddresses = LoadReceiveAddresses(cliWallet, addressService);
            receiveAddressIndex = 0;
        }

        private static int PickNode(string cliWalletConfigFolder, CliWalletConfig cliWalletConfig, List<Node> nodes, Random random, int nodeIndex)
        {
            if (settings.NodeSelectionMethod == NodeSelectionMethod.Static) //static
            {
                cliWalletConfig.WebAPI = settings.NodeToUseWhenStaticNodeSelectionMethod;
            }
            else
            {
                if (settings.NodeSelectionMethod == NodeSelectionMethod.Random) //random
                    nodeIndex = random.Next(nodes.Count);
                else if (settings.NodeSelectionMethod == NodeSelectionMethod.Sequential)//sequential
                    nodeIndex = (nodeIndex + 1) % nodes.Count;

                cliWalletConfig.WebAPI = nodes[nodeIndex].Url;
            }
            SaveCliWalletConfig(cliWalletConfigFolder, cliWalletConfig);

            return nodeIndex;
        }

        private static async Task<CliWalletConfig> LoadCliWalletConfig(string cliWalletConfigFolder)
        {
            //if not exist just run cli-wallet once which creates a `config.json`
            if (!File.Exists(MiscUtil.CliWalletConfig(cliWalletConfigFolder)))
            {
                CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, "");
                await commandLine.Run(); //ignore potential error because it also tries to connect to the default WebAPI
            }

            string jsonCliWalletConfig = File.ReadAllText(MiscUtil.CliWalletConfig(cliWalletConfigFolder));
            CliWalletConfig cliWalletConfig = JsonConvert.DeserializeObject<CliWalletConfig>(jsonCliWalletConfig);
            return cliWalletConfig;
        }

        private static void SaveCliWalletConfig(string cliWalletConfigFolder, CliWalletConfig cliWalletConfig)
        {
            string contents = JsonConvert.SerializeObject(cliWalletConfig, Formatting.Indented);
            File.WriteAllText(MiscUtil.CliWalletConfig(cliWalletConfigFolder), contents);
        }

        private static List<Address> LoadReceiveAddresses(CliWallet cliWallet, AddressService addressService)
        {
            List<Address> receiveAddresses = addressService.GetAll(Program.settings.VerifyIfReceiveAddressesExist).ToList();
            //only consider receiveAddresses of other persons
            receiveAddresses = receiveAddresses
                .Where(receiveAddress => !receiveAddress.Equals(cliWallet.ReceiveAddress)).ToList();

            int count = receiveAddresses.Count;

            receiveAddresses = receiveAddresses.Where(receiveAddress => receiveAddress.IsVerified).ToList();
            int delta = count - receiveAddresses.Count;

            if (delta > 0) Log.Logger.Information($"Skipped {delta} unverified receive addresses");
            Log.Logger.Information($"Total verified receive addresses: {receiveAddresses.Count}");

            if (receiveAddresses.Count == 0)
            {
                Log.Logger.Fatal($"No other receive addresses available at {Program.settings.UrlWalletReceiveAddresses}! Exiting...");
                CleanExit(1);
            }

            return receiveAddresses;
        }

        private static List<Node> LoadEnabledNodes(CliWallet cliWallet, NodeService nodeService)
        {
            List<Node> enabledNodes = nodeService
                .GetAll()
                .Where(node => node.Enabled)
                .ToList();

            if (enabledNodes.Count == 0 && settings.NodeSelectionMethod != NodeSelectionMethod.Static)
            {
                Log.Logger.Fatal($"No enabled nodes available at {Program.settings.UrlWalletNodes} and NodeSelectionMethod is not Static! Exiting...");
                CleanExit(1);
            }

            return enabledNodes;
        }

        private static string DetermineToWhichNodeToPledgeTo(string identityId, CliWalletConfig cliWalletConfig)
        {
            bool update = (identityId.Trim() == "");
            if (update)
            {
                return MiscUtil.GetIdentityId(cliWalletConfig.WebAPI);
            }
            else return ""; //just use empty string as cli-wallet will then by default pledge to the connected node
        }

        private static void WriteLockFile(string folder)
        {
            lockFile = MiscUtil.DefaultLockFile(folder);

            //check if program already started for this wallet by checking for lockfile
            if (File.Exists(lockFile))
            {
                bool found = true;
                try
                {
                    int id = Convert.ToInt32(File.ReadAllText(lockFile));
                    Process.GetProcessById(id);
                }
                catch
                {
                    found = false;
                }

                if (found)
                {
                    Log.Logger.Fatal($"Program already running for this wallet: {settings.CliWalletFullpath}");
                    CleanExit(1, false);
                }
                else File.Delete(lockFile);
            }

            //create lockfile to prevent running multiple program instances for same wallet
            File.WriteAllText(lockFile, Environment.ProcessId.ToString());
        }

        private static void RemoveLockFile(bool removeLockFile = true)
        {
            if (removeLockFile && File.Exists(lockFile)) File.Delete(lockFile);
        }

        private static string ParseArguments(string[] args)
        {
            if (args.Length > 1)
            {
                Log.Logger.Fatal("Error in arguments!");
                Log.Logger.Information("Syntax: <program> [settingsfile]");
                CleanExit(1);
            }

            string settingsFile;
            if (args.Length == 0)
            {
                string currentFolder = Directory.GetCurrentDirectory();
                settingsFile = MiscUtil.DefaultSettingsFile(currentFolder);
            }
            else
            {
                settingsFile = args[0];
            }

            return settingsFile;
        }

        static void CleanExit(int exitCode = 0, bool removeLockFile = true)
        {
            RemoveLockFile(removeLockFile);
            Environment.Exit(exitCode);
        }

        private static void InitLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(settings.Logging.MinimumLevel)
                .WriteTo.Console(restrictedToMinimumLevel: settings.Logging.Console.RestrictedToMinimumLevel)
                .WriteTo.File(
                    path: settings.Logging.File.Path,
                    rollingInterval: settings.Logging.File.RollingInterval,
                    rollOnFileSizeLimit: settings.Logging.File.RollOnFileSizeLimit,
                    restrictedToMinimumLevel: settings.Logging.File.RestrictedToMinimumLevel)
                //.WriteTo.Logger(
                //    x => x.Filter.ByIncludingOnly(y => y.ToString().ToLower().Contains("transaction"))
                //        .WriteTo.File("transaction.log",
                //            rollingInterval: RollingInterval.Day,
                //            rollOnFileSizeLimit: true
                //        ))
                .CreateLogger();
        }
    }
}
