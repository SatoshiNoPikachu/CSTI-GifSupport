using GifSupport.CstiGif;

namespace GifSupport.JsonData;

public static class Generator
{
    public static void Gen()
    {
        var data = new EditorJsonData.EditorJsonData(Plugin.PluginPath, "GIF-");
        data.AddType<CardDataGif>("Card", true);
        data.CreateJsonData();
    }
}