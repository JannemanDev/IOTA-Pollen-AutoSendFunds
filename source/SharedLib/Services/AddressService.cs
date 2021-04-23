using System.Collections.Generic;
using RestSharp;
using SharedLib.Interfaces;
using SharedLib.Models;

namespace SharedLib.Services
{
    public class AddressService : IAddressService
    {
        private readonly ICrudRepo<Address> _addressRepo;
        readonly RestClient _dashboardClient;

        public AddressService(ICrudRepo<Address> addressRepo, string dashboardUrl)
        {
            _addressRepo = addressRepo;
            _dashboardClient = new RestClient(dashboardUrl);
        }

        public HashSet<Address> GetAll(bool verifyIfReceiveAddressesExist = false)
        {
            HashSet<Address> receiveAddresses = _addressRepo.GetAll();

            if (verifyIfReceiveAddressesExist)
            {
                foreach (Address address in receiveAddresses)
                {
                    address.IsVerified = AddressExist(address.AddressValue);
                }
            }

            return receiveAddresses;
        }

        public bool Add(Address address)
        {
            return _addressRepo.Add(address);
        }

        public bool Delete(string addressValue)
        {
            return _addressRepo.Delete(addressValue);
        }

        public bool Contains(string addressValue)
        {
            return _addressRepo.Contains(addressValue);
        }

        public bool AddressExist(string addressValue)
        {
            string url = $"/api/address/{addressValue}";
            var request = new RestRequest(url, DataFormat.Json);

            //check if address really exist using the GoShimmer Dashboard
            IRestResponse response = _dashboardClient.Get(request);
            return response.IsSuccessful;
        }
    }
}
