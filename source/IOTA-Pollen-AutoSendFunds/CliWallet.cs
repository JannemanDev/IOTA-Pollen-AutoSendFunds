using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedLib;

namespace IOTA_Pollen_AutoSendFunds
{
    class CliWallet
    {
        public List<Balance> Balances { get; set; }
        public List<Address> Addresses { get; set; }

        public List<Address> ReceiveAddresses => Addresses.Where(address => address.IsReceive).ToList();
        public List<Address> SpentAddresses => Addresses.Where(address => !address.IsReceive).ToList();

        public CliWallet()
        {
            Balances = new List<Balance>();
            Addresses = new List<Address>();
        }

        public async Task UpdateBalances()
        {
            Balances.Clear();

            //run cli-wallet balance and capture output
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, "balance");
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Console.WriteLine("Error retrieving balance(s)!");
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
                string unitSubColumn = balanceSubColumns[1];

                //color
                string color = columns[2];

                //token name
                string tokenName = columns[3];

                Balance balance = new Balance(balanceStatus, balanceSubColumn, color, tokenName);
                Balances.Add(balance);
            }
        }

        public async Task SendFunds(int amount, Address destinationAddress, string tokenColor)
        {
            Console.WriteLine($"Sending {amount} {tokenColor} to {destinationAddress}");

            //run cli-wallet balance and capture output
            string arguments = $"send-funds -amount {amount} -dest-addr {destinationAddress.AddressValue} -color {tokenColor} -access-mana-id {Program.settings.AccessManaId} -consensus-mana-id {Program.settings.ConsensusManaId}";
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, arguments);
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Console.WriteLine(" Error sending funds!");
            }
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
                    int number;
                    string firstColumn = line.Split("\t")[0];
                    bool isNumber = int.TryParse(firstColumn, out number);
                    return isNumber;
                })
                .ToList();

            foreach (string line in lines)
            {
                List<string> columns = line.Split("\t", StringSplitOptions.RemoveEmptyEntries).ToList();
                int index = Convert.ToInt32(columns[0]); //skip/not used
                string addressValue = columns[1];
                bool isReceive = !Convert.ToBoolean(columns[2]);
                Address address;
                if (isReceive)
                {
                    address = new Address("Yourself", addressValue, true);
                }
                else address = new Address("Somebody else", addressValue, false);

                Addresses.Add(address);
            }

            if (commandLine.Result.ExitCode != 0)
            {
                Console.WriteLine(" Error getting wallet receive addresses!");
            }
        }

        public async Task RequestFunds()
        {
            UpdateBalances();

            int balanceAtStart = Balances.Where(balance => balance.TokenName.ToUpper() == "IOTA")
                .Select(balance => balance.BalanceValue)
                .Single();

            Console.WriteLine("\nRequesting IOTA tokens from the faucet... this takes a while...");

            //run cli-wallet balance and capture output
            string arguments = $"request-funds";
            CommandLine commandLine = new CommandLine(Program.settings.CliWalletFullpath, arguments);
            await commandLine.Run();

            if (commandLine.Result.ExitCode != 0)
            {
                Console.WriteLine("Failed!");
                return;
            }

            Console.WriteLine("Request complete... now waiting for IOTA tokens to arrive....");

            int seconds = 0;
            bool failed = false;
            do
            {
                if (seconds >= Program.settings.MaxWaitingTimeInSecondsForRequestingFunds)
                {
                    failed = true;
                    break;
                }

                Thread.Sleep(1000);
                UpdateBalances();

                if (Balances.Any(balance => balance.TokenName.ToUpper() == "IOTA" && balance.BalanceStatus == BalanceStatus.Pending))
                    Console.WriteLine("Found pending IOTA transaction! Waiting for it to complete...");

                seconds++;
            } while (!(Balances.Any(balance =>
                balance.TokenName.ToUpper() == "IOTA" && balance.BalanceStatus == BalanceStatus.Ok &&
                balance.BalanceValue > balanceAtStart)));

            if (failed) Console.WriteLine($"Requesting IOTA tokens failed to complete within {Program.settings.MaxWaitingTimeInSecondsForRequestingFunds} seconds");
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("CliWallet:\n");

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
