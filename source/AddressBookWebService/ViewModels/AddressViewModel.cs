using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.Validation;

namespace AddressBookWebService.ViewModels
{
    public class AddressViewModel
    {
        public string OwnerName { get; set; }

        [Required(ErrorMessage = "Address is required!")]
        [IotaAddress]
        public string AddressValue { get; set; }
    }
}
