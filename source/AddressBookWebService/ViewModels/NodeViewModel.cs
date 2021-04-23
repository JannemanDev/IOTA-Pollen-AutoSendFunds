using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.Validation;

namespace AddressBookWebService.ViewModels
{
    public class NodeViewModel
    {
        [Required(ErrorMessage = "Node is required!")]
        public string Url { get; set; }
    }
}
