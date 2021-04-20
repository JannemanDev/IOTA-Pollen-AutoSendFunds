using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.DTOs;
using AddressBookWebService.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SharedLib;
using SharedLib.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AddressBookWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
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

        // GET api/<AddressController>/5
        [HttpDelete("{addressValue}")]
        public DeleteAddressResponse Delete(string addressValue)
        {
            Console.WriteLine($"DELETE /api/address/{addressValue}");

            bool deleted = _addressService.DeleteAddress(addressValue);

            return new DeleteAddressResponse(true, "", deleted);
        }

        [HttpGet("search/{addressValue}")]
        public Address Get(string addressValue)
        {
            return _addressService.GetAllAddresses().Single(address => address.AddressValue == addressValue);
        }

        [HttpGet("isiotaaddress/{addressValue}")]
        public AddressVerification IsIotaAddress(string addressValue)
        {
            bool result = Address.IsIotaAddress(addressValue);
            string errorDescription = "";
            if (!result) errorDescription = "Invalid address";
            return new AddressVerification(result, errorDescription);
        }


        [HttpGet("addressexist/{addressValue}")]
        public AddressVerification AddressExist(string addressValue)
        {
            return new AddressVerification((_addressService.AddressExist(addressValue)));
        }

        // POST api/<AddressController>
        [HttpPost]
        public AddAddressResponse Post(AddressViewModel addressViewModel)
        {
            Console.WriteLine("POST /api/address");

            if (!ModelState.IsValid)
            {
                return new AddAddressResponse(false, "Not a valid AddressViewModel!", false);
            }

            Address address = new Address(addressViewModel.OwnerName, addressViewModel.AddressValue, true);
            bool updated = _addressService.AddAddress(address);

            return new AddAddressResponse(true, "", updated);
        }
    }
}
