using JsonFlatFileDataStore;
using System;
using SharedLib;
using System.Collections.Generic;
using System.Threading;

namespace TestJsonStore
{
    public class Program
    {
        static void Main(string[] args)
        {

            IAddressRepo addressRepo;
            addressRepo = new FileAddressRepo(@"C:\MyData\Persoonlijk\IOTA\IOTA-Pollen-AutoSendFunds\source\TestJsonStore\bin\Debug\net5.0\addresses.json");
            //addressRepo = new HttpAddressRepo("https://localhost:5001");

            // Get address collection
            HashSet<Address> addresses;
            ConsoleKeyInfo cki;
            do
            {
                addresses = addressRepo.GetAllAddresses();
                PrintHashSet(addresses);

                Console.ReadKey();

                // Create new address instance
                var address = new Address("Test", "1CjonvjuqufEwVSi3NvfjLLTJPSBS9bijqBBgTMH7NjF8", true);

                // Insert new address
                // Id is updated automatically to correct next value
                addressRepo.AddAddress(address);
                address = new Address("Test2", "2CjonvjuqufEwVSi3NvfjLLTJPSBS9bijqBBgTMH7NjF8", true);
                addressRepo.AddAddress(address);

                addresses = addressRepo.GetAllAddresses();
                PrintHashSet(addresses);
                cki = Console.ReadKey();
            } while (cki.Key != ConsoleKey.Escape);
        }

        static void PrintHashSet(HashSet<Address> hashSet)
        {
            Console.WriteLine(String.Join(",", hashSet));
        }
    }
}
