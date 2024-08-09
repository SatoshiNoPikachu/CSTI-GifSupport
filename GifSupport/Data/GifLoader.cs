using System.Collections.Generic;
using System.IO;

namespace GifSupport.Data;

public static class GifLoader
{
    private const string GifPath = "Resource/GIF";
    
    public static void LoadAllGif()
    {
        Plugin.Log.LogInfo("Load GIF...");
        
        var modDirs = new DirectoryInfo(BepInEx.Paths.PluginPath).GetDirectories();
        var gifs = new Dictionary<string, object>();
    
        foreach (var modDir in modDirs)
        {
            var path = Path.Combine(modDir.FullName, GifPath);
            if (!Directory.Exists(path)) continue;
    
            var files = new DirectoryInfo(path).GetFiles();
            foreach (var file in files)
            {
                var extension = file.Extension.ToLower();
                if (extension is not ".gif") continue;
    
                var name = Path.GetFileNameWithoutExtension(file.Name);
                if (gifs.ContainsKey(name))
                {
                    Plugin.Log.LogWarning($"{modDir.Name} not load gif same key {name}.");
                    continue;
                }

                var gif = Gif.LoadGif(file.FullName, name);
                if (gif is null)
                {
                    Plugin.Log.LogWarning($"Not load gif {name}.");
                    continue;
                }
                gifs.Add(name, gif);
            }
        }
    
        Database.AddData(typeof(Gif), gifs);
        
        Plugin.Log.LogInfo($"GIF loading completed, there are {gifs.Count} GIF.");
    }

    // public static async void LoadAllGifAsync()
    // {
    //     var modDirs = new DirectoryInfo(BepInEx.Paths.PluginPath).GetDirectories();
    //     var tasks = new Dictionary<string, Task<GifMeta>>();
    //
    //     foreach (var modDir in modDirs)
    //     {
    //         var path = Path.Combine(modDir.FullName, GifPath);
    //         if (!Directory.Exists(path)) continue;
    //
    //         var files = new DirectoryInfo(path).GetFiles();
    //         foreach (var file in files)
    //         {
    //             var extension = file.Extension.ToLower();
    //             if (extension is not ".gif") continue;
    //
    //             var name = Path.GetFileNameWithoutExtension(file.Name);
    //             if (tasks.ContainsKey(name))
    //             {
    //                 Plugin.Log.LogWarning($"{modDir.Name} not load gif same key {name}.");
    //                 continue;
    //             }
    //
    //             tasks.Add(name, Task.Run(() => GifMeta.LoadGifMeta(file.FullName, name)));
    //         }
    //     }
    //
    //     var result = await Task.WhenAll(tasks.Values);
    //     var gifs = result.ToDictionary<GifMeta, string, object>(meta => meta.Name, meta => meta.ToGif());
    //
    //     Database.AddData(typeof(Gif), gifs);
    // }
}