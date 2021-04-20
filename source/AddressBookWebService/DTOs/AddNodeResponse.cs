using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressBookWebService.DTOs
{
    public class AddNodeResponse
    {
        public bool Result { get; set; }
        public string ErrorDescription { get; set; }
        public bool Updated { get; set; }

        public AddNodeResponse(bool result, string errorDescription, bool updated)
        {
            Result = result;
            ErrorDescription = errorDescription;
            Updated = updated;
        }
    }
}
