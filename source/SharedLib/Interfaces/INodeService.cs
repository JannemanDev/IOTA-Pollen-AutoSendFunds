using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib.Models;

namespace SharedLib.Interfaces
{
    public interface INodeService
    {
        //Based on Node Repo contract
        public HashSet<Node> GetAll();
        public bool Add(Node node);
        public bool Delete(string url);
        public bool Contains(string url);

        //Todo: node reachable/pingable etc.
    }
}
