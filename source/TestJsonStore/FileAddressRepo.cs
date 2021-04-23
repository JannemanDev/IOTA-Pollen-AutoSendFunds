using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JsonFlatFileDataStore;
using SharedLib;
using SharedLib.Models;
using SharedLib.Services;

namespace TestJsonStore
{
    public class FileAddressRepo : IAddressRepo
    {
        private readonly DataStore _dataStore;
        private readonly string _nameCollection;
        private readonly bool _autoReload;

        public FileAddressRepo(string filename, bool autoReload = true)
        {
            // Open database (create new if file doesn't exist)
            _dataStore = new DataStore(filename);
            _nameCollection = "addresses"; //Todo: dynamic name
            _autoReload = autoReload;
        }

        public HashSet<Address> GetAllAddresses()
        {
            return LoadAllAddresses().AsQueryable().ToHashSet();
        }

        private IDocumentCollection<Address> LoadAllAddresses()
        {
            if (_autoReload) _dataStore.Reload();
            return _dataStore.GetCollection<Address>(_nameCollection);
        }

        public bool AddAddress(Address address)
        {
            IDocumentCollection<Address> addresses = LoadAllAddresses();
            bool addressExist = addresses.AsQueryable().FirstOrDefault(a => a.AddressValue == address.AddressValue) != null;

            if (addressExist) addresses.DeleteOne(a => a.AddressValue == address.AddressValue); //update if already exist
            return addresses.InsertOne(address);
        }

        public bool DeleteAddress(string addressValue)
        {
            return LoadAllAddresses().DeleteOne(address => address.AddressValue == addressValue);
        }

        public bool ContainsAddress(string addressValue)
        {
            return LoadAllAddresses().AsQueryable().FirstOrDefault(address => address.AddressValue == addressValue) != null;
        }
    }
}
