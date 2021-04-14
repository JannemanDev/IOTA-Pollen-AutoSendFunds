using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Pollen_AutoSendFunds.ExtensionMethods
{
    public static class IEnumerableExtensions
    {
        private static Random random = new Random();

        public static T RandomElement<T>(this IEnumerable<T> q)
        {
            return q.Skip(random.Next(q.Count())).FirstOrDefault();
        }
    }
}
