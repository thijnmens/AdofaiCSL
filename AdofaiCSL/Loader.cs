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

                bool success = AdofaiCSL.Load(modEntry);
                if (!success)
                {
                    return false;
                }
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
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                AppDomain.CurrentDomain.Load(data);
            }
        }
    }
}