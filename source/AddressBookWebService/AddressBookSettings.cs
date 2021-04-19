using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressBookWebService
{
    public class AddressBookSettings
    {
        public string FilenameWhereToStoreReceiveAddresses { get; set; }

        public string GoShimmerDashboardUrl { get; set; }

        public bool Available { get; set; }

        public string Description { get; set; }
    }
}
