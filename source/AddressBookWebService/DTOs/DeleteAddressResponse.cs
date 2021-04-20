using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressBookWebService.DTOs
{
    public class DeleteAddressResponse
    {
        public bool Result { get; set; }
        public string ErrorDescription { get; set; }
        public bool Deleted { get; set; }

        public DeleteAddressResponse(bool result, string errorDescription, bool deleted)
        {
            Result = result;
            ErrorDescription = errorDescription;
            Deleted = deleted;
        }
    }
}
