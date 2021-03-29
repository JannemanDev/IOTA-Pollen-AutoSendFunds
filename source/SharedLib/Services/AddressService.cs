using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public HashSet<Address> GetAllAddresses()
        {
            //check for addressUrl or local file
            string json;
            if (_addressUrl.ToUpper().StartsWith("HTTP://") || _addressUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_addressUrl);
                var request = new RestRequest("", DataFormat.None);

                IRestResponse response = client.Get(request);
                json = response.Content;
            }
            else json = File.ReadAllText(_addressUrl); //local file
            HashSet<Address> receiveAddresses = JsonConvert.DeserializeObject<HashSet<Address>>(json);

            foreach (Address address in receiveAddresses)
            {
                address.IsVerified = VerifyAddress(address.AddressValue);
            }

            return receiveAddresses;
        }

        public bool AddAddress(Address address)
        {
            HashSet<Address> addresses = GetAllAddresses();
            Address addressFound;
            bool updated = false;
            if (addresses.Contains(address))
            {
                addresses.Remove(address);
                updated = true;
            }
            addresses.Add(address);
            string json = JsonConvert.SerializeObject(addresses, Formatting.Indented);
            File.WriteAllText(_addressUrl, json);
            
            return updated;
        }

        public bool VerifyAddress(string addressValue)
        {
            var client = new RestClient(_dashboardUrl);
            var request = new RestRequest($"/api/address/{addressValue}", DataFormat.Json);

            IRestResponse response = client.Get(request);
            return response.IsSuccessful;
        }
   }
}
