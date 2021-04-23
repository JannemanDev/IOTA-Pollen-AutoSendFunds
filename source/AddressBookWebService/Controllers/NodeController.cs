using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.DTOs;
using AddressBookWebService.ViewModels;
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Services;

namespace AddressBookWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NodeController : ControllerBase
    {
        private readonly INodeService _nodeService;

        public NodeController(INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        // GET: api/<NodeController>
        [HttpGet("all")]
        public IEnumerable<Node> Get()
        {
            return _nodeService.GetAll();
        }

        // POST api/<NodeController>
        [HttpPost]
        public AddNodeResponse Post(NodeViewModel nodeViewModel)
        {
            //check if url is a public website and /info endpoint exist
            Console.WriteLine("POST /api/node");

            if (!ModelState.IsValid)
            {
                return new AddNodeResponse(false, "Not a valid NodeViewModel!", false);
            }

            //Todo: dubbel werk? geeft add niet al deze waarde?
            bool updated = _nodeService.Contains(nodeViewModel.Url);
            Node node = new Node(nodeViewModel.Url);
            bool result = _nodeService.Add(node);

            return new AddNodeResponse(result, result ? "" : "Error", updated && result);
        }

        // DELETE api/<NodeController>/5
        [HttpDelete]
        public void Delete([FromBody] string url)
        {
            _nodeService.Delete(url);
        }
    }
}
