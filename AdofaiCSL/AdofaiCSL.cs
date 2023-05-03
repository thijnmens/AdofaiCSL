using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace AdofaiCSL
{
    public class AdofaiCSL
    {
        public static string customSongsPath = $@"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}CustomSongs";
        public static UnityModManager.ModEntry mod;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                mod = modEntry;

                var harmony = new Harmony("com.thijnmens.adofaicls");
                harmony.PatchAll();

                if (!RDDirectory.Exists(customSongsPath))
                {
                    modEntry.Logger.Log("Directory \"CustomSongs\" does not exist, creating...");
                    RDDirectory.CreateDirectory(customSongsPath);
                };

                string[] songs = Directory.GetDirectories(customSongsPath);
                foreach (string songPath in songs)
                {
                    List<string> files = Directory.GetFiles(songPath, "*.adofai").ToList();
                    files.Remove(Path.Combine(songPath, "backup.adofai"));
                    if (files.Count == 0)
                    {
                        mod.Logger.Error($"Cannot find .adofai file in directory \"{songPath}\"");
                    }
                    else if (files.Count == 1)
                    {
                        string newFile = Path.Combine(songPath, "main.adofai");
                        File.Move(files[0], newFile);
                    }
                    else
                    {
                        mod.Logger.Warning($"Multiple .adofai files found in \"{songPath}\". Please make sure the correct file is called \"main.adofai\"");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                modEntry.Logger.Error(e.ToString());
                return false;
            }
        }
    }
}