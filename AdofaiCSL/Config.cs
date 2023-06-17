using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdofaiCSL
{
    internal static class Config
    {
        internal static Dictionary<string, string> Read(string configPath)
        {
            var config = new Dictionary<string, string>();

            using (var reader = new StreamReader(configPath))
            {
                try
                {
                    var data = reader.ReadToEnd().Trim();
                    var lines = data.Split('\n');
                    foreach (var line in lines)
                    {
                        var lineData = line.Split('=')
                            .Select(_ => _.Trim())
                            .Select(_ => _.ToLower())
                            .ToArray();
                        config.Add(lineData[0], lineData[1]);
                    }
                }
                catch
                {
                    AdofaiCSL.mod.Logger.Error($"Failed to parse config in \"{configPath}\"");
                }
            }

            return config;
        }

        internal static void Write(string configPath, Dictionary<string, string> data)
        {
            using (var writer = new StreamWriter(configPath))
            {
                try
                {
                    foreach (var kvp in data) writer.WriteLine($"{kvp.Key} = {kvp.Value}");
                }
                catch
                {
                    AdofaiCSL.mod.Logger.Error($"Failed to write config to \"{configPath}\"");
                }
            }
        }
    }
}