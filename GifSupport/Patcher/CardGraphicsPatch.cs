using GifSupport.CstiGif;
using HarmonyLib;

namespace GifSupport.Patcher;

[HarmonyPatch(typeof(CardGraphics))]
public static class CardGraphicsPatch
{
    [HarmonyPostfix, HarmonyPatch("Setup")]
    public static void Setup_Postfix(CardGraphics __instance)
    {
        CardDataGif.SetCardGif(__instance);
    }

    [HarmonyPostfix, HarmonyPatch("RefreshCookingStatus")]
    public static void RefreshCookingStatus_Postfix(CardGraphics __instance)
    {
        CardDataGif.SetCardGif(__instance);
    }
}