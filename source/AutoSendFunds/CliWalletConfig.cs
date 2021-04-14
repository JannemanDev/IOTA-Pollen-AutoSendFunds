using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IOTA_Pollen_AutoSendFunds
{
    public class CliWalletConfig
    {
        public string WebAPI { get; set; }
        [JsonProperty("basic_auth")]
        public BasicAuth BasicAuth { get; set; }

    }

    public class BasicAuth
    {
        public bool Enabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
