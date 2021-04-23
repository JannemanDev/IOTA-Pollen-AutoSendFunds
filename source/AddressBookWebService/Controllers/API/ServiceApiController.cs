using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.DTOs;
using Microsoft.Extensions.Options;

namespace AddressBookWebService.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceApiController : ControllerBase
    {
        private readonly AddressBookSettings _addressBookSettings;

        public ServiceApiController(IOptionsSnapshot<AddressBookSettings> addressBookSettings)
        {
            _addressBookSettings = addressBookSettings.Value;
        }

        [HttpGet("/api/available")]
        public ObjectResult Available()
        {
            bool available = _addressBookSettings.Available;
            string description = _addressBookSettings.Description;
            OnlineStatusResponse onlineStatusResponse = new OnlineStatusResponse(available, description);
            if (available) return Ok(onlineStatusResponse);
            else return StatusCode(503, onlineStatusResponse);
        }
    }
}
