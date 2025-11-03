using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Texel.Classes
{
    public static class MinecraftVersionStore
    {
        private static readonly string DataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "minecraft_versions.json");
        private static List<string> _cached;

        public static List<string> GetSupportedVersions()
        {
            if (_cached != null) return new List<string>(_cached);

            try
            {
                if (File.Exists(DataFilePath))
                {
                    var text = File.ReadAllText(DataFilePath);
                    var list = JsonConvert.DeserializeObject<List<string>>(text);
                    _cached = list ?? GetDefaultList();
                }
                else
                {
                    _cached = GetDefaultList();
                }
            }
            catch
            {
                _cached = GetDefaultList();
            }

            return new List<string>(_cached);
        }

        private static List<string> GetDefaultList()
        {
            return new List<string>
            {
                "1.21.0",
                "1.20.3",
                "1.20.2",
                "1.20.1",
                "1.20",
                "1.19.4",
                "1.19.3",
                "1.19",
                "1.18.2",
                "1.18",
                "1.17.1",
                "1.17",
                "1.16.5",
                "1.16",
                "1.15.2",
                "1.15",
                "1.14.4",
                "1.14",
                "1.13",
                "1.12",
                "1.11",
                "1.10",
                "1.9",
                "1.8"
            };
        }
    }
}
