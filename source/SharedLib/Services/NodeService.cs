using System.Collections.Generic;
using RestSharp;
using SharedLib.Interfaces;
using SharedLib.Models;

namespace SharedLib.Services
{
    public class NodeService : INodeService
    {
        private readonly ICrudRepo<Node> _nodeRepo;

        public NodeService(ICrudRepo<Node> nodeRepo)
        {
            _nodeRepo = nodeRepo;
        }

        public HashSet<Node> GetAll()
        {
            HashSet<Node> nodes = _nodeRepo.GetAll();

            return nodes;
        }

        public bool Add(Node node)
        {
            return _nodeRepo.Add(node);
        }

        public bool Delete(string url)
        {
            return _nodeRepo.Delete(url);
        }

        public bool Contains(string url)
        {
            return _nodeRepo.Contains(url);
        }
    }
}
