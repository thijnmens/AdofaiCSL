using ADOFAI;
using GDMiniJSON;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AdofaiCSL
{
    internal class Patches
    {
        [HarmonyPatch(typeof(scnCLS), "CreateFloors")]
        private static class scnCLS_CreateFloors_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(scnCLS __instance)
            {
                Dictionary<string, CustomLevelTile> loadedLevelTiles = (Dictionary<string, CustomLevelTile>)AccessTools.Field(typeof(scnCLS), "loadedLevelTiles").GetValue(__instance);
                Dictionary<string, bool> loadedLevelIsDeleted = (Dictionary<string, bool>)AccessTools.Field(typeof(scnCLS), "loadedLevelIsDeleted").GetValue(__instance);
                Dictionary<string, string> loadedLevelDirs = (Dictionary<string, string>)AccessTools.Field(typeof(scnCLS), "loadedLevelDirs").GetValue(__instance);

                string[] songs = Directory.GetDirectories(AdofaiCSL.customSongsPath);
                foreach (string songPath in songs)
                {
                    if (Directory.GetFiles(songPath, "*.adofai").Length == 0)
                    {
                        // TODO: Map folders
                        continue;
                    }

                    // Add tile
                    GameObject tile = UnityEngine.Object.Instantiate(__instance.tilePrefab, __instance.floorContainer);
                    tile.name = "CustomTile";
                    tile.GetComponent<scrFloor>().topGlow.gameObject.SetActive(value: true);
                    tile.GetComponent<scrFloor>().isLandable = true;
                    tile.transform.LocalMoveY(tile.transform.localPosition.y + (float)Mathf.FloorToInt(__instance.floorContainer.childCount / 2));
                    CustomLevelTile CLTile = tile.GetComponent<CustomLevelTile>();

                    // Add level
                    var level = new LevelDataCLS();
                    level.Setup();
                    if (level.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{songPath}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>))
                    {
                        __instance.loadedLevels.Add(songPath.Split(Path.DirectorySeparatorChar).Last(), level);
                    }

                    // Configure tile
                    CLTile.levelKey = songPath.Split(Path.DirectorySeparatorChar).Last();
                    CLTile.title.text = Regex.Replace(level.title, @"<[^>]+>| ", "").Trim();
                    CLTile.artist.text = Regex.Replace(level.artist, @"<[^>]+>| ", "").Trim();

                    if (level.previewIcon.Any())
                    {
                        CLTile.image.enabled = true;
                    }
                    else
                    {
                        CLTile.image.enabled = false;
                    }

                    loadedLevelTiles.Add(CLTile.levelKey, CLTile);
                    loadedLevelIsDeleted.Add(CLTile.levelKey, false);
                    loadedLevelDirs.Add(CLTile.levelKey, songPath);

                    // Add song
                    ADOBase.audioManager.FindOrLoadAudioClipExternal(Path.Combine(songPath, level.songFilename), false);

                    __instance.sortedLevelKeys.Add(CLTile.levelKey);
                    //__instance.SelectLevel(CLTile, true);
                }

                __instance.sortedLevelKeys.Sort();
                AccessTools.Field(typeof(scnCLS), "loadedLevelTiles").SetValue(__instance, loadedLevelTiles);
                AccessTools.Field(typeof(scnCLS), "loadedLevelIsDeleted").SetValue(__instance, loadedLevelIsDeleted);
                AccessTools.Field(typeof(scnCLS), "loadedLevelDirs").SetValue(__instance, loadedLevelDirs);
            }
        }
    }
}