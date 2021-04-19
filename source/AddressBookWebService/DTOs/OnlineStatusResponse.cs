using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressBookWebService.DTOs
{
    public class OnlineStatusResponse
    {
        public bool Available { get; set; }
        public string Description { get; set; }

        public OnlineStatusResponse(bool available, string description)
        {
            Available = available;
            Description = description;
        }
    }
}
