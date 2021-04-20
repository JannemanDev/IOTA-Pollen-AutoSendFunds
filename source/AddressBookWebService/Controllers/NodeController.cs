using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressBookWebService.DTOs;
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
        public IEnumerable<string> Get()
        {
            return _nodeService.GetAllNodes();
        }

        // POST api/<NodeController>
        [HttpPost]
        public AddNodeResponse Post([FromBody] string url)
        {
            //check if url is a public website and /info endpoint exist
            AddNodeResponse addNodeResponse = null;

            _nodeService.AddNode(url);
            return addNodeResponse;
        }

        // DELETE api/<NodeController>/5
        [HttpDelete]
        public void Delete([FromBody] string url)
        {
            _nodeService.DeleteNode(url);
        }
    }
}
