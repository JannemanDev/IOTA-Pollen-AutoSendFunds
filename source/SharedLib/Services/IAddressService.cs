using System.Collections.Generic;

namespace SharedLib.Services
{
    public interface IAddressService
    {
        public HashSet<Address> GetAllAddresses(bool verifyIfReceiveAddressesExist = false);
        public bool AddAddress(Address address);
        public bool AddressExist(string addressValue);
    }
}