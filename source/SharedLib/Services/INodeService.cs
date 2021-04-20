using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Services
{
    public interface INodeService
    {
        public HashSet<string> GetAllNodes();
        public bool AddNode(string url);
        public bool DeleteNode(string url);
    }
}
