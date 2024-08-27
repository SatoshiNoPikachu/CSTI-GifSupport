using GifSupport.CstiGif;
using HarmonyLib;

namespace GifSupport.Patcher;

[HarmonyPatch(typeof(InGameCardBase))]
public static class InGameCardBasePatch
{
    [HarmonyPostfix, HarmonyPatch("CardDescription")]
    public static void CardDescription_Postfix(InGameCardBase __instance, ref string __result,
        bool _IgnoreLiquid)
    {
        CardDataGif.SetCarDesc(__instance, ref __result, _IgnoreLiquid);
    }
}