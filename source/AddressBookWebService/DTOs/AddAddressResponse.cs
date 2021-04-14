using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressBookWebService.DTOs
{
    public class AddAddressResponse
    {
        public bool Result { get; set; }
        public string ErrorDescription { get; set; }
        public bool Updated { get; set; }

        public AddAddressResponse(bool result, string errorDescription, bool updated)
        {
            Result = result;
            ErrorDescription = errorDescription;
            Updated = updated;
        }
    }
}
