using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace GifSupport;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
internal class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "Pikachu.CSTIMod.GifSupport";
    public const string PluginName = "GifSupport";
    public const string PluginVersion = "1.0.1";

    public static Plugin Instance = null!;
    public static ManualLogSource Log = null!;
    private static readonly Harmony Harmony = new(PluginGuid);

    public static string PluginPath => Path.GetDirectoryName(Instance.Info.Location);

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        Harmony.PatchAll();
        Log.LogInfo($"Plugin {PluginName} is loaded!");
    }
}