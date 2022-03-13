using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using SharedLib.SettingsModels;

namespace SharedLib
{
    public static class MiscUtil
    {
        public static Settings LoadSettings(string settingsFile)
        {
            Log.Logger.Information($"Loading settings from {settingsFile}\n");

            string settingsJson = File.ReadAllText(settingsFile);

            Settings settings = JsonConvert.DeserializeObject<Settings>(settingsJson);

            //GoShimmerDashboardUrl is only needed to verify addresses
            //if not set fallback to the one that's provided by IF
            if (settings.GoShimmerDashboardUrl.Trim() == "") settings.GoShimmerDashboardUrl = Settings.defaultGoShimmerDashboardUrl;

            if (settings.NodeToUseWhenStaticNodeSelectionMethod.Trim() == "" && settings.NodeSelectionMethod == NodeSelectionMethod.Static)
            {
                throw new Exception("NodeSelectionMethod is static but NodeToUseWhenStaticNodeSelectionMethod is empty!");
            }

            // Set default settings
            // if not set use the API server
            bool updateUrlWalletReceiveAddresses = (settings.UrlWalletReceiveAddresses.Trim() == "");
            bool updateUrlWalletNode = (settings.UrlWalletNodes.Trim() == "");
            if (updateUrlWalletReceiveAddresses) settings.UrlWalletReceiveAddresses = Settings.defaultUrlApiServer;
            if (updateUrlWalletNode) settings.UrlWalletNodes = Settings.defaultUrlApiServer;

            //if AccessManaId and ConsensusManaId are not set current connected node will be used (see CliWallet.SendFunds)

            return settings;   
        }

        public static void WriteSettings(string settingsFile, Settings settings)
        {
            //write settings
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
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
