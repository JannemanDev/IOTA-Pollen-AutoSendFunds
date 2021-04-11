using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.ViewModels;
using Microsoft.Extensions.Configuration;
using SharedLib;
using SharedLib.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AddressBookWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAddressService _addressService;

        public AddressController(IConfiguration config, IAddressService addressService)
        {
            _config = config;
            _addressService = addressService;
        }

        // GET: api/<AddressController>
        [HttpGet("all")]
        public IEnumerable<Address> Get()
        {
            return _addressService.GetAllAddresses();
        }

        // GET api/<AddressController>/5
        [HttpGet("{id}")]
        public Address Get(int id)
        {
            return _addressService.GetAllAddresses().ElementAt(id);
        }

        [HttpGet("search/{addressValue}")]
        public Address Get(string addressValue)
        {
            return _addressService.GetAllAddresses().Single(address => address.AddressValue == addressValue);
        }

        [HttpGet("validate/{addressValue}")]
        public AddressVerification Validate(string addressValue)
        {
            bool result;
            result = Address.IsIotaAddress(addressValue);
            if (!result) return new AddressVerification(false, "Not a valid IOTA address!");
            result = _addressService.AddressExist(addressValue);
            if (!result) return new AddressVerification(false, "Address not available!");
            else return new AddressVerification(true, "");
        }

        // POST api/<AddressController>
        [HttpPost]
        public void Post(AddressViewModel addressViewModel)
        {
            Console.WriteLine("test");
        }

        // PUT api/<AddressController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AddressController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
