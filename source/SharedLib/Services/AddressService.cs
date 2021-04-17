using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SharedLib;

namespace SharedLib.Services
{
    public class AddressService : IAddressService
    {
        private readonly string _addressUrl;
        private readonly string _dashboardUrl;

        public AddressService(string addressUrl, string dashboardUrl)
        {
            _addressUrl = addressUrl;
            _dashboardUrl = dashboardUrl;
        }

        public HashSet<Address> GetAllAddresses(bool verifyIfReceiveAddressesExist = false)
        {
            //check for addressUrl or local file
            string json;
            if (_addressUrl.ToUpper().StartsWith("HTTP://") || _addressUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_addressUrl);
                var request = new RestRequest("/api/address/all", DataFormat.None);

                IRestResponse response = client.Get(request);
                json = response.Content;
            }
            else
            {
                if (!File.Exists(_addressUrl)) json = "[]";
                else json = File.ReadAllText(_addressUrl); //local file
            }
            HashSet<Address> receiveAddresses = JsonConvert.DeserializeObject<HashSet<Address>>(json);

            if (verifyIfReceiveAddressesExist)
            {
                foreach (Address address in receiveAddresses)
                {
                    address.IsVerified = AddressExist(address.AddressValue);
                }
            }

            return receiveAddresses;
        }

        public bool AddAddress(Address address)
        {
            HashSet<Address> addresses = GetAllAddresses();
            bool updated = false;
            string json;

            if (_addressUrl.ToUpper().StartsWith("HTTP://") || _addressUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_addressUrl);
                var request = new RestRequest("/api/address", DataFormat.None);

                json = JsonConvert.SerializeObject(address, Formatting.Indented);
                request.AddJsonBody(json);
                Console.WriteLine($"{_addressUrl} request:{json}");
                IRestResponse response = client.Post(request);
                json = response.Content;
                Console.WriteLine($"json response: {json}");
                Console.WriteLine($"{response.ErrorMessage}");
                Console.WriteLine($"{response}");
                JObject obj = JObject.Parse(json);

                return (bool)obj["updated"];
            }
            else
            {
                if (addresses.Contains(address))
                {
                    addresses.Remove(address);
                    updated = true;
                }
                addresses.Add(address);
                json = JsonConvert.SerializeObject(addresses, Formatting.Indented);
                File.WriteAllText(_addressUrl, json);
            }

            return updated;
        }

        public bool AddressExist(string addressValue)
        {
            var client = new RestClient(_dashboardUrl);
            string url = $"/api/address/{addressValue}";
            var request = new RestRequest(url, DataFormat.Json);

            IRestResponse response = client.Get(request);
            return response.IsSuccessful;
        }
    }
}
