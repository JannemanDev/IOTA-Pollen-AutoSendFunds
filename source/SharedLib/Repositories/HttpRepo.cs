using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SharedLib;
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Services;

namespace SharedLib.Repositories
{
    public class HttpRepo<T> : ICrudRepo<T> where T : Entity
    {
        private readonly string _url;
        private readonly RestClient _client;
        private readonly string _entityName;

        public HttpRepo(string url, string entityName)
        {
            _url = url;
            _client = new RestClient(_url);
            _entityName = entityName;
        }

        public HashSet<T> GetAll()
        {
            var request = new RestRequest($"/api/{_entityName}/all", DataFormat.None);

            IRestResponse response = _client.Get(request);
            string json = response.Content;
            HashSet<T> objects = JsonConvert.DeserializeObject<HashSet<T>>(json);
            return objects;
        }

        public bool Add(T obj)
        {
            var request = new RestRequest($"/api/{_entityName}", DataFormat.None);

            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            request.AddJsonBody(json);
            Console.WriteLine($"{_url} request:{json}");
            IRestResponse response = _client.Post(request);
            json = response.Content;
            Console.WriteLine($"json response: {json}");
            Console.WriteLine($"{response.ErrorMessage}");
            Console.WriteLine($"{response}");
            JObject result = JObject.Parse(json);

            return (bool)result["updated"];
        }

        public bool Delete(string id)
        {
            string path = $"/api/{_entityName}/{id}";
            var request = new RestRequest(path, DataFormat.None);

            Console.WriteLine($"{_url} request:{path}");
            IRestResponse response = _client.Delete(request);
            string json = response.Content;
            Console.WriteLine($"json response: {json}");
            Console.WriteLine($"{response.ErrorMessage}");
            Console.WriteLine($"{response}");
            JObject jsonObj = JObject.Parse(json);

            return (bool)jsonObj["deleted"];
        }

        public bool Contains(string id)
        {
            string path = $"api/{_entityName}/search/{id}";
            var request = new RestRequest(path, DataFormat.Json);

            IRestResponse response = _client.Get(request);
            return response.IsSuccessful;
        }
    }
}
