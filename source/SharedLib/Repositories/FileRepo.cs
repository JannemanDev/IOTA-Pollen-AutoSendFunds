using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JsonFlatFileDataStore;
using SharedLib;
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Services;

namespace SharedLib.Repositories
{
    //Implemented using https://github.com/ttu/json-flatfile-datastore
    public class FileRepo<T> : ICrudRepo<T> where T : Entity
    {
        private readonly DataStore _dataStore;
        private readonly string _nameCollection;
        public bool AutoReload { get; set; }

        public FileRepo(string filename, bool autoReload = true)
        {
            // Open database (create new if file doesn't exist)
            _dataStore = new DataStore(filename);
            _nameCollection = Path.GetFileNameWithoutExtension(filename);
            AutoReload = autoReload;
        }

        public HashSet<T> GetAll()
        {
            return LoadAll().AsQueryable().ToHashSet();
        }

        private IDocumentCollection<T> LoadAll()
        {
            if (AutoReload) _dataStore.Reload();
            return _dataStore.GetCollection<T>(_nameCollection);
        }

        public bool Add(T obj)
        {
            IDocumentCollection<T> addresses = LoadAll();
            bool addressExist = addresses.AsQueryable().FirstOrDefault(a => a.Equals(obj)) != null;

            if (addressExist) addresses.DeleteMany(a => a.Equals(obj)); //update if already exist
            return addresses.InsertOne(obj);
        }

        public bool Delete(string id)
        {
            return LoadAll().DeleteMany(a => a.Id == id);
        }

        public bool Contains(string id)
        {
            return LoadAll().AsQueryable().FirstOrDefault(a => a.Id == id) != null;
        }
    }
}
