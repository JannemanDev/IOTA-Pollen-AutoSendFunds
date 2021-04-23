using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SharedLib;
using SharedLib.Services;

namespace TestJsonStore
{
    public class HttpAddressRepo : IAddressRepo
    {
        private readonly string _addressUrl;
        private readonly RestClient _addressClient;

        public HttpAddressRepo(string addressUrl)
        {
            _addressUrl = addressUrl;
            _addressClient = new RestClient(_addressUrl);
        }

        public HashSet<Address> GetAllAddresses()
        {
            var request = new RestRequest("/api/address/all", DataFormat.None);

            IRestResponse response = _addressClient.Get(request);
            string json = response.Content;
            HashSet<Address> receiveAddresses = JsonConvert.DeserializeObject<HashSet<Address>>(json);
            return receiveAddresses;
        }

        public bool AddAddress(Address address)
        {
            var request = new RestRequest("/api/address", DataFormat.None);

            string json = JsonConvert.SerializeObject(address, Formatting.Indented);
            request.AddJsonBody(json);
            Console.WriteLine($"{_addressUrl} request:{json}");
            IRestResponse response = _addressClient.Post(request);
            json = response.Content;
            Console.WriteLine($"json response: {json}");
            Console.WriteLine($"{response.ErrorMessage}");
            Console.WriteLine($"{response}");
            JObject obj = JObject.Parse(json);

            return (bool)obj["updated"];
        }

        public bool DeleteAddress(string addressValue)
        {
            string path = $"/api/address/{addressValue}";
            var request = new RestRequest(path, DataFormat.None);

            Console.WriteLine($"{_addressUrl} request:{path}");
            IRestResponse response = _addressClient.Delete(request);
            string json = response.Content;
            Console.WriteLine($"json response: {json}");
            Console.WriteLine($"{response.ErrorMessage}");
            Console.WriteLine($"{response}");
            JObject obj = JObject.Parse(json);

            return (bool)obj["deleted"];
        }

        public bool ContainsAddress(string addressValue)
        {
            string path = $"/search/{addressValue}";
            var request = new RestRequest(path, DataFormat.Json);

            IRestResponse response = _addressClient.Get(request);
            return response.IsSuccessful;
        }
    }
}
