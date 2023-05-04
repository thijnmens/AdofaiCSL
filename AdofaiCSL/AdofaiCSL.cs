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
        public static int offset = 0;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                mod = modEntry;

                var config = Config.Read(Path.Combine(AppContext.BaseDirectory, "Mods/AdofaiCSL/config.cfg"));
                offset = int.Parse(config["offset"]);

                mod.OnGUI += (UnityModManager.ModEntry _) =>
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(200f));
                    GUILayout.Label("Tile Offset");
                    if (GUILayout.Button("-", GUILayout.Width(50f)))
                    {
                        offset--;
                    }
                    GUILayout.Label(offset.ToString());
                    if (GUILayout.Button("+", GUILayout.Width(50f)))
                    {
                        offset++;
                    }
                    GUILayout.EndHorizontal();
                };

                mod.OnSaveGUI += (UnityModManager.ModEntry _) =>
                {
                    Config.Write(Path.Combine(AppContext.BaseDirectory, "Mods/AdofaiCSL/config.cfg"), new Dictionary<string, string>() { { "offset", offset.ToString() } });
                };

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
                    if (Directory.GetFiles(songPath, "*.pack").Length > 0)
                    {
                        // Pack
                        string[] packSongs = Directory.GetDirectories(songPath);
                        foreach (string packSongPath in packSongs)
                        {
                            List<string> files = Directory.GetFiles(packSongPath, "*.adofai").ToList();
                            files.Remove(Path.Combine(packSongPath, "backup.adofai"));
                            if (files.Count == 1)
                            {
                                // Rename File
                                string newFile = Path.Combine(packSongPath, "main.adofai");
                                File.Move(files[0], newFile);
                            }
                            else
                            {
                                // Warn user about main.adofai missing
                                if (!File.Exists(Path.Combine(packSongPath, "main.adofai")))
                                {
                                    // multiple files and no main.adofai
                                    mod.Logger.Critical($"Multiple .adofai files found in \"{packSongPath}\". Please rename the correct file to \"main.adofai\"");
                                }
                            }
                        }
                    }
                    else if (Directory.GetFiles(songPath, "*.adofai").Length > 0)
                    {
                        // Song
                        List<string> files = Directory.GetFiles(songPath, "*.adofai").ToList();
                        files.Remove(Path.Combine(songPath, "backup.adofai"));
                        if (files.Count == 1)
                        {
                            // Rename File
                            string newFile = Path.Combine(songPath, "main.adofai");
                            File.Move(files[0], newFile);
                        }
                        else
                        {
                            // Warn user about main.adofai missing
                            if (!File.Exists(Path.Combine(songPath, "main.adofai")))
                            {
                                // multiple files and no main.adofai
                                mod.Logger.Critical($"Multiple .adofai files found in \"{songPath}\". Please rename the correct file to \"main.adofai\"");
                            }
                        }
                    }
                    else
                    {
                        // No .pack or .adofai file
                        mod.Logger.Error($"Cannot find .adofai or .pack file in directory \"{songPath}\"");
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