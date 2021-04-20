using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;

namespace SharedLib.Services
{
    public class NodeService : INodeService
    {
        private readonly string _nodeUrl;
        private readonly HashSet<string> _nodes;

        private readonly Policy _fileWriterPolicy;

        public NodeService(string nodeUrl)
        {
            _nodeUrl = nodeUrl;
            _nodes = LoadAllNodes();

            _fileWriterPolicy = Policy
                .Handle<IOException>()
                .RetryForever((exception, retryCount, context) =>
                {
                    Console.WriteLine($"File {_nodeUrl} in use. Retry count: {retryCount}");
                });
        }

        private HashSet<string> LoadAllNodes()
        {
            //check for addressUrl or local file
            string json;
            if (_nodeUrl.ToUpper().StartsWith("HTTP://") || _nodeUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_nodeUrl);
                var request = new RestRequest("/api/node/all", DataFormat.None);

                IRestResponse response = client.Get(request);
                json = response.Content;
            }
            else
            {
                if (!File.Exists(_nodeUrl)) json = "[]";
                else json = File.ReadAllText(_nodeUrl); //local file
            }
            HashSet<string> nodeUrls = JsonConvert.DeserializeObject<HashSet<string>>(json);

            return nodeUrls;
        }

        public HashSet<string> GetAllNodes()
        {
            return _nodes;
        }

        public bool AddNode(string url)
        {
            bool result = false;
            string json;

            if (_nodeUrl.ToUpper().StartsWith("HTTP://") || _nodeUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_nodeUrl);
                var request = new RestRequest("/api/node", DataFormat.None);

                json = JsonConvert.SerializeObject(url, Formatting.Indented);
                request.AddJsonBody(json);
                Console.WriteLine($"{_nodeUrl} request:{json}");
                IRestResponse response = client.Post(request);
                json = response.Content;
                Console.WriteLine($"json response: {json}");
                Console.WriteLine($"{response.ErrorMessage}");
                Console.WriteLine($"{response}");
                JObject obj = JObject.Parse(json);

                return response.IsSuccessful;
            }
            else
            {
                if (!_nodes.Contains(url))
                {
                    _nodes.Add(url);
                    json = JsonConvert.SerializeObject(_nodes, Formatting.Indented);
                    _fileWriterPolicy.Execute(() => { File.WriteAllText(_nodeUrl, json); });
                    result = true;
                }
            }

            return result;
        }

        public bool DeleteNode(string url)
        {
            bool deleted = false;
            string json;

            if (_nodeUrl.ToUpper().StartsWith("HTTP://") || _nodeUrl.ToUpper().StartsWith("HTTPS://"))
            {
                var client = new RestClient(_nodeUrl);
                string path = $"/api/node/{url}";
                var request = new RestRequest(path, DataFormat.None);

                Console.WriteLine($"{_nodeUrl} request:{path}");
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
                if (_nodes.Contains(url))
                {
                    _nodes.Remove(url);
                    deleted = true;
                }
                json = JsonConvert.SerializeObject(_nodes, Formatting.Indented);
                _fileWriterPolicy.Execute(() => { File.WriteAllText(_nodeUrl, json); });
            }

            return deleted;
        }
    }
}
