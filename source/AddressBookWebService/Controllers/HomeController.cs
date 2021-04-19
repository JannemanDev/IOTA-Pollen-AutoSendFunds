using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.DTOs;
using AddressBookWebService.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedLib;
using SharedLib.Services;

namespace AddressBookWebService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IAddressService _addressService;
        private readonly AddressBookSettings _addressBookSettings;

        public HomeController(ILogger<HomeController> logger, IConfiguration config, IAddressService addressService, IOptionsSnapshot<AddressBookSettings> addressBookSettings)
        {
            _logger = logger;
            _config = config;
            _addressService = addressService;
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

        public IActionResult Index()
        {
            ViewBag.Addresses = _addressService.GetAllAddresses();
            ViewBag.Updated = null;

            return View(new AddressViewModel() { AddressValue = "", OwnerName = "Anonymous" });
        }

        [HttpPost]
        public IActionResult Index(AddressViewModel addressViewModel)
        {
            //Todo: viewbag vullen of refactoren zonder viewbag
            if (!ModelState.IsValid)
            {
                ViewBag.Updated = null;
                return View(addressViewModel);
            }

            addressViewModel.OwnerName = addressViewModel.OwnerName.Trim();
            if (addressViewModel.OwnerName == "") addressViewModel.OwnerName = "Anonymous";

            Address address = new Address(addressViewModel.OwnerName, addressViewModel.AddressValue, true, true);
            ViewBag.Updated = (Boolean)_addressService.AddAddress(address);

            ViewBag.Addresses = _addressService.GetAllAddresses();

            return View(new AddressViewModel() { AddressValue = "", OwnerName = "Anonymous" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
