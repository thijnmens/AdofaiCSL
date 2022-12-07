using HarmonyLib;
using System;

namespace AdofaiCSL
{
    internal class Patches
    {
        [HarmonyPatch(typeof(scrController), "QuitToMainMenu")]
        private static class scrController_QuitToMainMenu_Patch
        {
            [HarmonyPrefix]
            private static bool Prefix(scnEditor __instance)
            {
                if (AdofaiCSL.playingCustom)
                {
                    RDUtils.SetGarbageCollectionEnabled(enabled: true);
                    ADOBase.audioManager.StopLoadingMP3File();
                    AdofaiCSL.playingCustom = false;
                    ADOBase.LoadScene(GCNS.sceneSplash);
                    return false;
                }
                return true;
            }
        }
    }
}
