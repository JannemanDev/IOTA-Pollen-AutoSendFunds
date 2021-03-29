using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOTA_Pollen_AutoSendFunds.ExtensionMethods;
using Newtonsoft.Json;
using RestSharp;
using SharedLib;
using SharedLib.Services;
using SimpleBase;

/*
 *
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
 *  hosten op ....? fhict of docker lokaal?
 */

//Todo:
//-console output gaat door elkaar agv async stuff
//-address validation (sometimes its seed or only 1 column etc..)
//-check for correct version icm addresses
//-request funds when exist but empty
//-test requestFunds
//-test filtering on tokenstosent
//-test with colored tokens -> color token is uniek (=address), tokenname hoeft niet uniek te zijn?!

namespace IOTA_Pollen_AutoSendFunds
{
    class Program
    {
        public static Settings settings;

        //private static List<Address> wallets;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Dashboard - IOTA-Pollen-AutoSendFunds v0.1\n");
            Console.WriteLine(" Escape to quit");
            Console.WriteLine(" Space to pause\n");

            settings = MiscUtil.LoadSettings(Directory.GetCurrentDirectory());

            //wallets = Test();
            CliWallet cliWallet = new CliWallet();
            await cliWallet.UpdateBalances();

            if (cliWallet.Balances.Count == 0)
            {
                Console.WriteLine("No balance(s) exist! Exiting...");
                return;
            }

            await cliWallet.UpdateAddresses();

            AddressService addressService = new AddressService(Program.settings.UrlWalletReceiveAddresses,"");
            List<Address> receiveAddresses = addressService.GetAllAddresses().ToList();
            if (receiveAddresses.Count == 0)
            {
                Console.WriteLine(
                    $"No receive addresses available at {Program.settings.UrlWalletReceiveAddresses}! Exiting...");
                return;
            }

            //await cliWallet.RequestFunds();

            //only consider receiveAddresses of other persons
            receiveAddresses = receiveAddresses
                .Where(receiveAddresses => !cliWallet.ReceiveAddresses.Contains(receiveAddresses)).ToList();

            Console.WriteLine(cliWallet);

            Random random = new Random();
            int receiveAddressIndex = -1;

            while (true)
            {
                //pick random token
                Balance balance = cliWallet.Balances
                    .Where(balance => balance.BalanceValue > 0)
                    .Where(balance =>
                        settings.TokensToSent.Contains(balance.TokenName)) //only consider tokens from the settings
                    .RandomElement();

                if (balance == null)
                {
                    Console.WriteLine(
                        $"No non-empty balances for tokens {String.Join(", ", settings.TokensToSent)} available!");
                    if (settings.StopWhenNoBalanceWithCreditIsAvailable)
                    {
                        Console.WriteLine("Exiting!");
                        return;
                    }
                    else
                    {
                        await cliWallet.RequestFunds();
                    }
                }
                else
                {
                    //random amount respecting available balanceValue
                    int min = Math.Min(settings.MinAmountToSend, balance.BalanceValue);
                    int max = Math.Min(settings.MaxAmountToSend, balance.BalanceValue);
                    int amount = random.Next(min, max);

                    //pick next destination address
                    if (settings.PickRandomDestinationAddress) //random
                        receiveAddressIndex = random.Next(receiveAddresses.Count);
                    else //sequential
                        receiveAddressIndex = (receiveAddressIndex + 1) % receiveAddresses.Count;

                    Address address = receiveAddresses[receiveAddressIndex];
                    try
                    {
                        cliWallet.SendFunds(amount, address, balance.Color);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        //throw;
                    }

                    if (settings.TimeoutInSecondsBetweenTransactions > 0)
                    {
                        Console.WriteLine(
                            $"\nSleeping for {settings.TimeoutInSecondsBetweenTransactions} seconds between transactions...\n");
                        Thread.Sleep(settings.TimeoutInSecondsBetweenTransactions * 1000);
                    }
                }

                bool pause = false;
                while (Console.KeyAvailable || (pause))
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Escape) return;
                    if (cki.Key == ConsoleKey.Spacebar)
                    {
                        if (pause) pause = false; //continue
                        else
                        {
                            Console.WriteLine("Pausing... Press <space> to continue, B = Balance, <escape> to quit!");
                            pause = true;
                        }
                    }

                    if (cki.Key == ConsoleKey.B) //show balance
                    {
                        await cliWallet.UpdateBalances();
                        Console.WriteLine(cliWallet);
                    }
                }
            }
        }
    }
}
