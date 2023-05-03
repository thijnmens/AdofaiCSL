using ADOFAI;
using GDMiniJSON;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AdofaiCSL
{
    internal class Patches
    {
        private static int tilesAdded = 0;
        private static int workshoplevels = 0;

        private static CustomLevelTile CreateCustomLevelTile(ref scnCLS instance, string songPath)
        {
            // Add tile
            GameObject tile = UnityEngine.Object.Instantiate(instance.tilePrefab, instance.floorContainer);
            tile.name = "CustomTile";
            tile.GetComponent<scrFloor>().topGlow.gameObject.SetActive(value: true);
            tile.GetComponent<scrFloor>().isLandable = true;
            tile.transform.LocalMoveY(tile.transform.localPosition.y + (float)Mathf.FloorToInt(workshoplevels / 2 + tilesAdded));
            CustomLevelTile CLTile = tile.GetComponent<CustomLevelTile>();

            // Add level
            var level = new LevelDataCLS();
            level.Setup();
            if (level.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{songPath}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>))
            {
                instance.loadedLevels.Add(songPath.Split(Path.DirectorySeparatorChar).Last(), level);
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

            // Add song
            ADOBase.audioManager.FindOrLoadAudioClipExternal(Path.Combine(songPath, level.songFilename), false);

            tilesAdded++;
            return CLTile;
        }

        private static CustomLevelTile CreateCustomPackLevelTile(ref scnCLS instance, string songPath, ref CustomLevelTile CPTile, out LevelDataCLS packLevel)
        {
            // Add tile
            GameObject packSongTile = UnityEngine.Object.Instantiate(instance.tilePrefab, instance.floorContainer);
            packSongTile.name = "CustomTile";
            packSongTile.GetComponent<scrFloor>().topGlow.gameObject.SetActive(value: true);
            packSongTile.GetComponent<scrFloor>().isLandable = true;
            packSongTile.transform.LocalMoveY(192843928); // move far from screen
            CustomLevelTile CPLTile = packSongTile.GetComponent<CustomLevelTile>();

            // Add level
            packLevel = new LevelDataCLS();
            packLevel.Setup();
            if (packLevel.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{songPath}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>))
            {
                packLevel.parentFolderName = CPTile.levelKey;
                instance.loadedLevels.Add(songPath.Split(Path.DirectorySeparatorChar).Last(), packLevel);
            }

            // Configure tile
            CPLTile.levelKey = songPath.Split(Path.DirectorySeparatorChar).Last();
            CPLTile.title.text = Regex.Replace(packLevel.title, @"<[^>]+>| ", "").Trim();
            CPLTile.artist.text = Regex.Replace(packLevel.artist, @"<[^>]+>| ", "").Trim();

            if (packLevel.previewIcon.Any())
            {
                CPLTile.image.enabled = true;
            }
            else
            {
                CPLTile.image.enabled = false;
            }
            return CPLTile;
        }

        private static CustomLevelTile CreateCustomPackTile(ref scnCLS instance, string songPath, out FolderDataCLS packData)
        {
            // Add tile
            GameObject packTile = Object.Instantiate(instance.tilePrefab, instance.floorContainer);
            packTile.name = "CustomTile";
            packTile.GetComponent<scrFloor>().topGlow.gameObject.SetActive(value: true);
            packTile.GetComponent<scrFloor>().isLandable = true;
            packTile.transform.LocalMoveY(packTile.transform.localPosition.y + (float)Mathf.FloorToInt(workshoplevels / 2 + tilesAdded));
            CustomLevelTile CPTile = packTile.GetComponent<CustomLevelTile>();

            // Configure tile
            Dictionary<string, string> packConfig = Config.ReadPack(Directory.GetFiles(songPath, "*.pack").First());

            CPTile.levelKey = $"CustomFolder:{songPath.Split(Path.DirectorySeparatorChar).Last()}";
            CPTile.title.text = packConfig["title"].Trim();
            CPTile.artist.text = packConfig["artist"].Trim();
            CPTile.image.enabled = true;

            packData = new FolderDataCLS(
                packConfig["title"],
                int.Parse(packConfig["difficulty"]),
                packConfig["artist"],
                packConfig["author"],
                packConfig["description"],
                packConfig["image"],
                packConfig["icon"],
                packConfig["color"].HexToColor()
            );

            tilesAdded++;
            return CPTile;
        }

        [HarmonyPatch(typeof(scnCLS), "CreateFloors")]
        private static class scnCLS_CreateFloors_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(scnCLS __instance)
            {
                Dictionary<string, CustomLevelTile> loadedLevelTiles = (Dictionary<string, CustomLevelTile>)AccessTools.Field(typeof(scnCLS), "loadedLevelTiles").GetValue(__instance);
                Dictionary<string, bool> loadedLevelIsDeleted = (Dictionary<string, bool>)AccessTools.Field(typeof(scnCLS), "loadedLevelIsDeleted").GetValue(__instance);
                Dictionary<string, string> loadedLevelDirs = (Dictionary<string, string>)AccessTools.Field(typeof(scnCLS), "loadedLevelDirs").GetValue(__instance);

                tilesAdded = 0;
                workshoplevels = __instance.loadedLevels.Count;

                string[] songs = Directory.GetDirectories(AdofaiCSL.customSongsPath);
                foreach (string songPath in songs)
                {
                    if (Directory.GetFiles(songPath, "*.adofai").Length == 0)
                    {
                        // Custom Pack

                        var CPTile = CreateCustomPackTile(ref __instance, songPath, out FolderDataCLS packData);

                        foreach (string packSongPath in Directory.GetDirectories(songPath))
                        {
                            // Pack Level Tile

                            var CPLTile = CreateCustomPackLevelTile(ref __instance, packSongPath, ref CPTile, out LevelDataCLS packLevel);

                            packData.containingLevels.Add(CPLTile.levelKey, packLevel);
                            loadedLevelTiles.Add(CPLTile.levelKey, CPLTile);
                            loadedLevelIsDeleted.Add(CPLTile.levelKey, false);
                            loadedLevelDirs.Add(CPLTile.levelKey, packSongPath);
                        }

                        __instance.loadedLevels.Add($"CustomFolder:{songPath.Split(Path.DirectorySeparatorChar).Last()}", packData);
                        loadedLevelTiles.Add(CPTile.levelKey, CPTile);
                        loadedLevelIsDeleted.Add(CPTile.levelKey, false);
                        loadedLevelDirs.Add(CPTile.levelKey, songPath);
                        __instance.sortedLevelKeys.Add(CPTile.levelKey);

                        continue;
                    }

                    // Custom Level

                    var CLTile = CreateCustomLevelTile(ref __instance, songPath);

                    loadedLevelTiles.Add(CLTile.levelKey, CLTile);
                    loadedLevelIsDeleted.Add(CLTile.levelKey, false);
                    loadedLevelDirs.Add(CLTile.levelKey, songPath);
                    __instance.sortedLevelKeys.Add(CLTile.levelKey);
                }

                __instance.sortedLevelKeys.Sort();
                AccessTools.Field(typeof(scnCLS), "loadedLevelTiles").SetValue(__instance, loadedLevelTiles);
                AccessTools.Field(typeof(scnCLS), "loadedLevelIsDeleted").SetValue(__instance, loadedLevelIsDeleted);
                AccessTools.Field(typeof(scnCLS), "loadedLevelDirs").SetValue(__instance, loadedLevelDirs);

                __instance.gemTop.MoveY(__instance.gemTopY + tilesAdded - 1);
                __instance.gemTopY += tilesAdded - 1;
            }
        }

        [HarmonyPatch(typeof(scrController), "QuitToMainMenu")]
        private static class scrController_QuitToMainMenu_Patch
        {
            [HarmonyPrefix]
            private static void Prefix(scrController __instance)
            {
                GCS.customLevelPaths = null;
            }
        }
    }
}