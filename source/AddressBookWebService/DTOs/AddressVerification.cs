using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressBookWebService.DTOs
{
    public class AddressVerification
    {
        public bool Result { get; set; }
        public string ErrorDescription { get; set; }

        public AddressVerification(bool result) : this(result,"")
        {
        }

        public AddressVerification(bool result, string errorDescription)
        {
            Result = result;
            ErrorDescription = errorDescription;
        }
    }
}
