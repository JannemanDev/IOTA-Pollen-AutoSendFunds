using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace SharedLib
{
    public static class MiscUtil
    {
        public static Settings LoadSettings(string path)
        {
            path = Path.Combine(path, " ").TrimEnd();

            string settingsJson = File.ReadAllText(@$"{path}settings.json");

            return JsonConvert.DeserializeObject<Settings>(settingsJson);

        }
    }
}
