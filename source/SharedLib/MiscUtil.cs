using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace SharedLib
{
    public static class MiscUtil
    {
        public static Settings LoadSettings(string filename)
        {
            string settingsJson = File.ReadAllText(filename);

            return JsonConvert.DeserializeObject<Settings>(settingsJson);
        }

        public static string DefaultSettingsFile(string path="")
        {
            path = Path.Combine(path, " ").TrimEnd();
            return @$"{path}settings.json";
        }

        public static string DefaultLockFile(string path = "")
        {
            path = Path.Combine(path, " ").TrimEnd();
            return @$"{path}autosendfunds.lock";
        }

        public static string CliWalletConfig(string path = "")
        {
            path = Path.Combine(path, " ").TrimEnd();
            return @$"{path}config.json";
        }

        public static string GetIdentityId(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest("/info", DataFormat.None);

            IRestResponse response = client.Get(request);
            string json = response.Content;
            JObject obj = JObject.Parse(json);

            return (string)obj["identityID"];
        }
    }
}
