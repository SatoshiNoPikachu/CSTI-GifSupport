using GifSupport.Data;
using HarmonyLib;

namespace GifSupport.Patcher;

[HarmonyPatch(typeof(GameLoad))]
public static class GameLoadPatch
{
    [HarmonyPostfix, HarmonyPatch("LoadMainGameData")]
    public static void LoadMainGameData_Postfix()
    {
        Loader.LoadAllData(DataCatalog.Catalog);
    }
}