using System;
using System.IO;
using UnityModManagerNet;

namespace AdofaiCSL
{
    internal static class Loader
    {
        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                // LoadAssembly("Mods/AdofaiSRM/Newtonsoft.Json.dll");

                var success = AdofaiCSL.Load(modEntry);
                if (!success) return false;
                return true;
            }
            catch (Exception e)
            {
                modEntry.Logger.Error(e.ToString());
                return false;
            }
        }

        private static void LoadAssembly(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                AppDomain.CurrentDomain.Load(data);
            }
        }
    }
}