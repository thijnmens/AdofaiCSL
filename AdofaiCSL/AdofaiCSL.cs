using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace AdofaiCSL
{
    public class AdofaiCSL
    {
        public static UnityModManager.ModEntry mod;
        public static string[] songs;
        public static string query = "";

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                mod = modEntry;
                string dataPath = $@"{Persistence.DataPath}{Path.DirectorySeparatorChar}customlevels.txt";
                string customSongsPath = $@"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}CustomSongs";

                var harmony = new Harmony("com.thijnmens.adofaicls");
                harmony.PatchAll();

                if (!RDDirectory.Exists(customSongsPath))
                {
                    modEntry.Logger.Log("Directory \"CustomSongs\" does not exist, creating...");
                    RDDirectory.CreateDirectory(customSongsPath);
                };

                songs = Directory.GetDirectories(customSongsPath);

                modEntry.OnGUI += OnGUI;

                return true;
            }
            catch (Exception e)
            {
                modEntry.Logger.Error(e.ToString());
                return false;
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Search:");
            query = GUILayout.TextField(query).ToLower();
            GUILayout.Label("Custom Levels");
            foreach (string song in songs)
            {
                string[] files = Directory.GetFiles($@"{song}{Path.DirectorySeparatorChar}", "*.adofai");
                if (files.Length > 0)
                {
                    // Single Level
                    if (song.Split('\\').Last().ToLower().Contains(query))
                    {
                        bool clicked = GUILayout.Button(song.Split('\\').Last());
                        if (clicked)
                        {
                            files.ToList().Remove($@"{song}{Path.DirectorySeparatorChar}backup.adofai");
                            ADOBase.controller.LoadCustomWorld($@"{files.First()}");
                        }
                    }
                }
                else
                {
                    // Mappack
                    GUILayout.Label(song.Split('\\').Last());
                    string[] packLevels = Directory.GetDirectories(song);
                    foreach (string packSong in packLevels)
                    {
                        if (packSong.Split('\\').Last().ToLower().Contains(query))
                        {
                            files = Directory.GetFiles($@"{packSong}{Path.DirectorySeparatorChar}", "*.adofai");
                            if (files.Length > 0)
                            {
                                bool clicked = GUILayout.Button(packSong.Split('\\').Last());
                                if (clicked)
                                {
                                    files.ToList().Remove($@"{packSong}{Path.DirectorySeparatorChar}backup.adofai");
                                    ADOBase.controller.LoadCustomWorld($@"{files.First()}");
                                }
                            }
                        }
                    };
                    GUILayout.Label("Custom Levels");
                }
            }
            if (GUILayout.Button("Refresh"))
            {
                songs = Directory.GetDirectories($@"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}CustomSongs");
            }
        }
    }
}