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
        [JsonProperty("basicAuth")]
        public BasicAuth BasicAuth { get; set; }
        [JsonProperty("reuse_addresses")]
        public bool ReuseAddresses { get; set; }
        [JsonProperty("faucetPowDifficulty")]
        public int FaucetPowDifficulty { get; set; }
        [JsonProperty("assetRegistryNetwork")]
        public string AssetRegistryNetwork { get; set; }
    }

    public class BasicAuth
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
