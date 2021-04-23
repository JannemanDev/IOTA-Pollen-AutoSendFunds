using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Services;

namespace SharedLib.Repositories.Addresses
{
    public static class RepoFactory
    {
        public static ICrudRepo<Address> CreateAddressRepo(string url)
        {
            if (url.ToUpper().StartsWith("HTTP://") || url.ToUpper().StartsWith("HTTPS://"))
                return new HttpRepo<Address>(url,"address");
            else return new FileRepo<Address>(url);
        }

        public static ICrudRepo<Node> CreateNodeRepo(string url)
        {
            if (url.ToUpper().StartsWith("HTTP://") || url.ToUpper().StartsWith("HTTPS://"))
                return new HttpRepo<Node>(url, "node");
            else return new FileRepo<Node>(url);
        }
    }
}
