﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdofaiCSL
{
    internal static class Config
    {
        internal static Dictionary<string, string> ReadPack(string configPath)
        {
            var config = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(configPath))
            {
                try
                {
                    string data = reader.ReadToEnd().Trim();
                    string[] lines = data.Split('\n');
                    foreach (string line in lines)
                    {
                        string[] lineData = line.Split('=')
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
    }
}