using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib;
using SharedLib.Models;

namespace SharedLib.Interfaces
{
    public interface ICrudRepo<T>
    {
        public HashSet<T> GetAll();
        public bool Add(T obj);
        public bool Delete(string id);
        public bool Contains(string id);

    }
}
