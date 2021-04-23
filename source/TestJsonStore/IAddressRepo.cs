using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib;

namespace TestJsonStore
{
    public interface IAddressRepo
    {
        public HashSet<Address> GetAllAddresses();
        public bool AddAddress(Address address);
        public bool DeleteAddress(string addressValue);
        public bool ContainsAddress(string addressValue);
    }
}
