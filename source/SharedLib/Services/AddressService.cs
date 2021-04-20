using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;
using SharedLib;

namespace SharedLib.Services
{
    public class AddressService : IAddressService
    {
        private readonly string _addressUrl;
        private readonly string _dashboardUrl;
        private readonly HashSet<Address> _addresses;

        private readonly Policy _fileWriterPolicy;

        public AddressService(string addressUrl, string dashboardUrl)
        {
            _addressUrl = addressUrl;
            _dashboardUrl = dashboardUrl;
            _addresses = LoadAllAddresses();

            _fileWriterPolicy = Policy
                .Handle<IOException>()
                .RetryForever((exception, retryCount, context) =>
                {
                    Console.WriteLine($"File {_addressUrl} in use. Retry count: {retryCount}");
                });
        }
        
        private HashSet<Address> LoadAllAddresses(bool verifyIfReceiveAddressesExist = false)
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

            if (verifyIfReceiveAddressesExist) VerifyAddresses();

            return receiveAddresses;
        }

        public HashSet<Address> GetAllAddresses(bool verifyIfReceiveAddressesExist = false)
        {
            if (verifyIfReceiveAddressesExist) VerifyAddresses();

            return _addresses;
        }


        private void VerifyAddresses()
        {
            foreach (Address address in _addresses)
            {
                address.IsVerified = AddressExist(address.AddressValue);
            }
        }

        public bool AddAddress(Address address)
        {
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
                if (_addresses.Contains(address))
                {
                    _addresses.Remove(address);
                    updated = true;
                }
                _addresses.Add(address);
                json = JsonConvert.SerializeObject(_addresses, Formatting.Indented);
                _fileWriterPolicy.Execute(() => { File.WriteAllText(_addressUrl, json); });
            }

            return updated;
        }

        public bool DeleteAddress(string addressValue)
        {
            bool deleted = false;
            string json;

            if (_addressUrl.ToUpper().StartsWith("HTTP://") || _addressUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_addressUrl);
                string path = $"/api/address/{addressValue}";
                var request = new RestRequest(path, DataFormat.None);

                Console.WriteLine($"{_addressUrl} request:{path}");
                IRestResponse response = client.Delete(request);
                json = response.Content;
                Console.WriteLine($"json response: {json}");
                Console.WriteLine($"{response.ErrorMessage}");
                Console.WriteLine($"{response}");
                JObject obj = JObject.Parse(json);

                return (bool)obj["deleted"];
            }
            else
            {
                //search hashset for addressValue
                Address address = _addresses.FirstOrDefault(address => address.AddressValue == addressValue);

                if (address != null)
                {
                    _addresses.Remove(address);
                    deleted = true;
                }
                json = JsonConvert.SerializeObject(_addresses, Formatting.Indented);
                _fileWriterPolicy.Execute(() => { File.WriteAllText(_addressUrl, json); });
            }

            return deleted;
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
