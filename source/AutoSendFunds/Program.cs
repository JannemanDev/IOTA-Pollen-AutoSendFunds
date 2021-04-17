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
using SharedLib;
using SharedLib.Services;
using SimpleBase;

/*
 * v Show only errors / or also option(?)
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
 * auto wallet create
 * auto (re)request funds
 * make an option for: use same receive adress or use unspent adres
 * save wallet address in api repo
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

namespace IOTA_Pollen_AutoSendFunds
{
    class Program
    {
        public static Settings settings;

        public static string lockFile = "";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Dashboard - IOTA-Pollen-AutoSendFunds v0.1\n");
            Console.WriteLine(" Escape to quit");
            Console.WriteLine(" Space to pause\n");

            if (args.Length > 1)
            {
                Console.WriteLine("Error in arguments!");
                Console.WriteLine("Syntax: <program> [settingsfile]");
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

            if (!File.Exists(settingsFile))
            {
                Console.WriteLine($"Settingsfile {settingsFile} not found!");
                CleanExit(1);
            }

            //settingsFile = "c:\\temp\\iota\\newwallet3\\settings.json"; //override for testing purposes

            Console.WriteLine($"Loading settings from {settingsFile}\n");

            settings = MiscUtil.LoadSettings(settingsFile);

            //first check if wallet exist
            if (!CliWallet.Exist(settings.CliWalletFullpath))
            {
                Console.WriteLine($"Wallet not found! File does not exist: {CliWallet.DefaultWalletFile(settings.CliWalletFullpath)}");
                CleanExit(1);
            }

            string cliWalletConfigFolder = Path.GetDirectoryName(settings.CliWalletFullpath);

            //update empty default values in settings
            // 1. AccessManaId and ConsensusManaId
            string jsonCliWalletConfig = File.ReadAllText(MiscUtil.CliWalletConfig(cliWalletConfigFolder));
            CliWalletConfig cliWalletConfig = JsonConvert.DeserializeObject<CliWalletConfig>(jsonCliWalletConfig);

            bool updateAccessManaId = (settings.AccessManaId.Trim() == "");
            bool updateConsensusManaId = (settings.ConsensusManaId.Trim() == "");
            if (updateAccessManaId || updateConsensusManaId)
            {
                string identityId = MiscUtil.GetIdentityId(cliWalletConfig.WebAPI);
                if (updateAccessManaId) settings.AccessManaId = identityId;
                if (updateConsensusManaId) settings.ConsensusManaId = identityId;
            }

            // 2. GoShimmerDashboardUrl
            if (settings.GoShimmerDashboardUrl.Trim() == "")
            {
                Uri myUri = new Uri(cliWalletConfig.WebAPI);
                string scheme = myUri.Scheme;
                if (scheme.Trim() == "") scheme = "http";
                string host = $"{scheme}{Uri.SchemeDelimiter}{myUri.Host}:8081";
                settings.GoShimmerDashboardUrl = host;
            }

            //write settings back
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));

            lockFile = MiscUtil.DefaultLockFile(cliWalletConfigFolder);
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
                    Console.WriteLine($"Program already running for this wallet: {settings.CliWalletFullpath}");
                    CleanExit(1);
                }
                else File.Delete(lockFile);
            }

            //create lockfile to prevent running multiple program instances for same wallet
            await File.WriteAllTextAsync(lockFile, Environment.ProcessId.ToString());

            //Todo: temporary solution for error: "The SSL connection could not be established, see inner exception."
            //       when using RestSharp.
            //      Postman generates a warning: "Unable to verify the first certificate"
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;
            
            CliWallet cliWallet = new CliWallet();
            await cliWallet.UpdateAddresses();

            AddressService addressService = new AddressService(Program.settings.UrlWalletReceiveAddresses, Program.settings.GoShimmerDashboardUrl);

            if (settings.PublishReceiveAddress) addressService.AddAddress(cliWallet.ReceiveAddress);

            List<Address> receiveAddresses = addressService.GetAllAddresses(Program.settings.VerifyIfReceiveAddressesExist).ToList();
            //only consider receiveAddresses of other persons
            receiveAddresses = receiveAddresses
                .Where(receiveAddress => !receiveAddress.Equals(cliWallet.ReceiveAddress)).ToList();

            if (receiveAddresses.Count == 0)
            {
                Console.WriteLine(
                    $"No other receive addresses available at {Program.settings.UrlWalletReceiveAddresses}! Exiting...");
                CleanExit(1);
            }

            Console.WriteLine(cliWallet);

            Random random = new Random();
            int receiveAddressIndex = -1;

            while (true)
            {
                var balance = new { Color = "", Value = 0 }; //init to make compiler happy
                bool resultRequestFunds = true;
                do
                {
                    //pick random token
                    balance = cliWallet.Balances
                        //.Where(balance => balance.BalanceStatus == BalanceStatus.Ok)
                        .Where(balance => settings.TokensToSent.Contains(balance.TokenName)) //only consider tokens from the settings
                        .GroupBy(balance => balance.Color)  //groupby Color (which is unique)
                        .Select(group => new
                        { //sum all ok/pend balances for each token
                            Color = group.Key,
                            Value = group.Sum(x => x.BalanceValue)
                        })
                        .Where(colorToken => colorToken.Value > settings.MinAmountToSend) //only with enough balance
                        .RandomElement();

                    if (balance == null)
                    {
                        Console.WriteLine($"None of these tokens {String.Join(", ", settings.TokensToSent)} have a balance with at least {settings.MinAmountToSend}!");

                        if (settings.StopWhenNoBalanceWithCreditIsAvailable)
                        {
                            Console.WriteLine("Exiting!");
                            CleanExit(0);
                        }
                        else
                        {
                            resultRequestFunds = await cliWallet.RequestFunds();
                        }
                    }
                } while (balance == null);

                if (!resultRequestFunds) CleanExit(1);

                //random amount respecting available balanceValue
                int min = Math.Min(settings.MinAmountToSend, balance.Value);
                int max = Math.Min(settings.MaxAmountToSend, balance.Value);
                int amount = random.Next(min, max);

                //pick next destination address
                if (settings.PickRandomDestinationAddress) //random
                    receiveAddressIndex = random.Next(receiveAddresses.Count);
                else //sequential
                    receiveAddressIndex = (receiveAddressIndex + 1) % receiveAddresses.Count;

                Address address = receiveAddresses[receiveAddressIndex];
                try
                {
                    await CliWallet.SendFunds(amount, address, balance.Color);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    //throw;
                }

                int remainingTimeBetweenTransactions = settings.WaitingTimeInSecondsBetweenTransactions;

                bool pause = false;
                while (Console.KeyAvailable || (pause) || (remainingTimeBetweenTransactions > 0))
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
                                Console.WriteLine(
                                    "Pausing... Press <space> to continue, B = Balance, <escape> to quit!");
                                pause = true;
                            }
                        }

                        if (cki.Key == ConsoleKey.B) //show balance
                        {
                            await cliWallet.UpdateBalances();
                            Console.WriteLine(cliWallet);
                        }
                    }

                    if (!pause && remainingTimeBetweenTransactions > 0)
                    {
                        if (remainingTimeBetweenTransactions == settings.WaitingTimeInSecondsBetweenTransactions)
                            Console.Write($"\nSleeping between transactions:");
                        Console.Write($" {remainingTimeBetweenTransactions}");

                        Thread.Sleep(1000);
                        remainingTimeBetweenTransactions--;

                        if (remainingTimeBetweenTransactions == 0) Console.WriteLine();
                    }
                }

                await cliWallet.UpdateBalances();
            }
        }

        static void CleanExit(int exitCode = 0)
        {
            if (File.Exists(lockFile)) File.Delete(lockFile);
            Environment.Exit(exitCode);
        }
    }
}
