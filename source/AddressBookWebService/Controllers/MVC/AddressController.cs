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
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Services;

namespace AddressBookWebService.Controllers
{
    public class AddressController : Controller
    {
        private readonly ILogger<AddressController> _logger;
        private readonly IAddressService _addressService;

        public AddressController(ILogger<AddressController> logger, IAddressService addressService)
        {
            _logger = logger;
            _addressService = addressService;
        }
        
        public IActionResult Index()
        {
            ViewBag.Addresses = _addressService.GetAll();
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

            Address address = new Address(addressViewModel.OwnerName, addressViewModel.AddressValue, true);
            ViewBag.Updated = (Boolean)_addressService.Add(address);

            ViewBag.Addresses = _addressService.GetAll();

            return View(new AddressViewModel() { AddressValue = "", OwnerName = "Anonymous" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
