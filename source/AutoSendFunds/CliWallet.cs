using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SharedLib;
using SharedLib.Models;

namespace IOTA_Pollen_AutoSendFunds
{
    class CliWallet
    {
        public List<Balance> Balances { get; private set; }
        public List<Address> Addresses { get; private set; }

        public Address ReceiveAddress => Addresses.FirstOrDefault();

        public CliWallet()
        {
            Balances = new List<Balance>();
            Addresses = new List<Address>();
        }

        public static string DefaultWalletFile(string cliWalletFullPath = "")
        {
            return Path.Combine(Path.GetDirectoryName(cliWalletFullPath), "wallet.dat");
        }

        public static bool Exist(string cliWalletFullPath)
        {
            string cliWalletFile = DefaultWalletFile(cliWalletFullPath);
            return File.Exists(cliWalletFile);
        }

        public static async Task Init()
        {
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, "init");
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Log.Logger.Error($"Error creating wallet: {commandLine.Result.StandardError}");
            }
        }

        public static async Task ConsolidateFunds()
        {
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, "consolidate-funds");
            await commandLine.Run();
        }

        public async Task UpdateBalances()
        {
            Balances.Clear();

            //run cli-wallet balance and capture output
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, "balance");
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Log.Logger.Error("Error retrieving balance(s)!");
            }

            //split in lines and only consider lines with a balance
            List<string> lines = commandLine.Result.StandardOutput.Split("\n", StringSplitOptions.None).ToList();
            lines = lines.Where(line => line.Trim().StartsWith("[") && line.Contains("]"))
                .ToList();

            foreach (string line in lines)
            {
                //process line with tab as separator
                List<string> columns = line.Split("\t", StringSplitOptions.RemoveEmptyEntries).ToList();

                //status
                BalanceStatus balanceStatus;
                string status = columns[0]
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace(" ", "");

                if (status.ToUpper() == "OK") balanceStatus = BalanceStatus.Ok;
                else if (status.ToUpper() == "PEND") balanceStatus = BalanceStatus.Pending;
                else throw new Exception($" Unknown balance status {status}");

                //balance
                List<string> balanceSubColumns = columns[1].Split(" ").ToList();
                int balanceSubColumn = Convert.ToInt32(balanceSubColumns[0]);
                string unitSubColumn = "";
                if (balanceSubColumns.Count > 1) //is optional (currently not used)
                    unitSubColumn = balanceSubColumns[1];

                //color
                string color = columns[2];

                //token name
                string tokenName = columns[3];

                Balance balance = new Balance(balanceStatus, balanceSubColumn, color, tokenName);
                Balances.Add(balance);
            }
        }

        public async Task<bool> SendFunds(int amount, Address destinationAddress, string tokenColor, string accessManaId, string consensusManaId)
        {
            bool result = true;

            Log.Logger.Information($"Sending {amount} {TokenNameForColor(tokenColor)} to {destinationAddress}");

            //run cli-wallet balance and capture output
            string arguments = $"send-funds -amount {amount} -dest-addr {destinationAddress.AddressValue} -color {tokenColor}";
            //if AccessManaId or ConsensusManaId are not set current connected node will be used by default by cli-wallet
            if (accessManaId != "") arguments += $" -access-mana-id {accessManaId}";
            if (consensusManaId != "") arguments += $" -consensus-mana-id {consensusManaId}";

            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, arguments);
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Log.Logger.Error(" Error sending funds!");
                result = false;
            }

            //waiting till status is Ok
            Log.Logger.Information($"Sending complete... waiting for balance status of {TokenNameForColor(tokenColor)} to return to OK...");

            bool failed = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                if (stopwatch.Elapsed.TotalSeconds >= Program.settings.MaxWaitingTimeInSecondsForRequestingFunds)
                {
                    failed = true;
                    break;
                }

                Thread.Sleep(1000);
                await UpdateBalances();

            } while (Balances.Any(balance =>
                balance.TokenName.ToUpper() == tokenColor.ToUpper() && balance.BalanceStatus != BalanceStatus.Ok));

            if (failed) Log.Logger.Error($"Sending {amount} {TokenNameForColor(tokenColor)} to {destinationAddress} failed to complete within {Program.settings.MaxWaitingTimeInSecondsForRequestingFunds} seconds");

            result = result && !failed;

            return result;
        }

        public async Task UpdateAddresses()
        {
            Addresses.Clear();

            //run cli-wallet balance and capture output
            string arguments = $"address -list";
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, arguments);
            await commandLine.Run();

            //split in lines and only consider lines starting with a number
            List<string> lines = commandLine.Result.StandardOutput.Split("\n", StringSplitOptions.None).ToList();
            lines = lines.Where(line =>
                {
                    string firstColumn = line.Split("\t")[0];
                    bool isNumber = int.TryParse(firstColumn, out _);
                    return isNumber;
                })
                .ToList();

            foreach (string line in lines)
            {
                List<string> columns = line.Split("\t", StringSplitOptions.RemoveEmptyEntries).ToList();
                int index = Convert.ToInt32(columns[0]); //skip/not used
                string addressValue = columns[1];
                bool isSpent = Convert.ToBoolean(columns[2]); //ignore
                Address address = new Address(Program.settings.WalletName, addressValue, true);

                Addresses.Add(address);
            }

            if (commandLine.Result.ExitCode != 0)
            {
                Log.Logger.Error(" Error getting wallet receive addresses!");
            }
        }

        public string TokenNameForColor(string tokenColor)
        {
            string tokenName = Balances
                    .First(balance => balance.Color == tokenColor) //color is unique so when multiple (2) balances (in case of PEND and Ok balance) just take first
                    .TokenName;

            return tokenName;
        }

        public int TotalBalanceOfTokenByColor(string color)
        {
            //includes Ok and Pending balances
            int balanceAtStart = Balances
                                    .Where(balance => balance.Color.ToUpper() == color.ToUpper())
                                    .Select(balance => balance.BalanceValue)
                                    .DefaultIfEmpty(0)
                                    .Sum();
            return balanceAtStart;
        }

        public async Task<bool> RequestFunds()
        {
            await UpdateBalances();

            int balanceAtStart = TotalBalanceOfTokenByColor("IOTA");

            Log.Logger.Information("\nRequesting IOTA tokens from the faucet... this takes a while...");

            //run cli-wallet balance and capture output
            string arguments = $"request-funds";
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, arguments);
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Log.Logger.Error("Failed!");
                return false;
            }

            Log.Logger.Information("Request complete... now waiting for IOTA tokens to arrive....");

            bool failed = false;
            bool firstPrintFlag = true;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                if (stopwatch.Elapsed.TotalSeconds >= Program.settings.MaxWaitingTimeInSecondsForRequestingFunds)
                {
                    failed = true;
                    break;
                }

                Thread.Sleep(1000);
                await UpdateBalances();

                if (Balances.Any(balance =>
                    balance.TokenName.ToUpper() == "IOTA" && balance.BalanceStatus == BalanceStatus.Pending))
                {
                    if (firstPrintFlag)
                    {
                        Log.Logger.Information("Found pending IOTA transaction! Waiting for it to complete...");
                        firstPrintFlag = false;
                    }
                }
            } while ((TotalBalanceOfTokenByColor("IOTA") <= balanceAtStart) || (Balances.Any(balance =>
                balance.TokenName.ToUpper() == "IOTA" && balance.BalanceStatus != BalanceStatus.Ok))); ;

            if (!firstPrintFlag) Log.Logger.Information("");

            if (failed) Log.Logger.Error($"Requesting IOTA tokens failed to complete within {Program.settings.MaxWaitingTimeInSecondsForRequestingFunds} seconds");
            else Log.Logger.Information($" It took {Math.Round(stopwatch.Elapsed.TotalSeconds, 0)} seconds");

            return !failed;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"CliWallet '{Program.settings.WalletName}' ReceiveAddress {ReceiveAddress.AddressValue}:\n");

            if (Balances.Count == 0) stringBuilder.Append(" Empty! No balances found!");
            else
            {
                foreach (Balance balance in Balances)
                {
                    stringBuilder.Append(" " + balance.ToString() + "\n");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
