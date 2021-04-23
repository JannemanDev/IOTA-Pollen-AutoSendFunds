using System.Collections.Generic;
using SharedLib.Models;

namespace SharedLib.Interfaces
{
    public interface IAddressService
    {
        //Based on Address Repo contract
        public HashSet<Address> GetAll(bool verifyIfReceiveAddressesExist = false);
        public bool Add(Address address);
        public bool Delete(string addressValue);
        public bool Contains(string addressValue);

        //Extra services
        public bool AddressExist(string addressValue);
    }
}