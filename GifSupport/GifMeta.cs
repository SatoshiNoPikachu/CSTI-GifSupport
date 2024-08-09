// using System.Collections.Generic;
// using UnityEngine;
// using UnityGifDecoder;
//
// namespace GifSupport;
//
// public class GifMeta(string name)
// {
//     public string Name { get; private set; } = name;
//
//     public List<FrameMeta> Frames { get; private set; } = [];
//
//     public static GifMeta LoadGifMeta(string path, string name)
//     {
//         var meta = new GifMeta(name);
//
//         using var stream = new GifStream(path);
//         while (stream.HasMoreData)
//         {
//             switch (stream.CurrentToken)
//             {
//                 case GifStream.Token.Image:
//                     var image = stream.ReadImage();
//                     var frame = new FrameMeta(stream.Header.width, stream.Header.height, image.colors,
//                         image.DelaySeconds);
//                     meta.AddFrame(frame);
//                     break;
//
//                 case GifStream.Token.Comment:
//                     var commentText = stream.ReadComment();
//                     Plugin.Log.LogInfo(commentText);
//                     break;
//
//                 default:
//                     stream.SkipToken();
//                     break;
//             }
//         }
//
//         return meta;
//     }
//
//     public void AddFrame(FrameMeta meta)
//     {
//         Frames.Add(meta);
//     }
//
//     public Gif ToGif()
//     {
//         var gif = new Gif(Name);
//         foreach (var meta in Frames)
//         {
//             gif.AddFrame(meta.ToFrame());
//         }
//
//         return gif;
//     }
//
//     public class FrameMeta(int width, int height, Color32[] colors, float delay)
//     {
//         public Gif.Frame ToFrame()
//         {
//             var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
//             tex.SetPixels32(colors);
//             tex.Apply();
//
//             var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
//             return new Gif.Frame(sprite, delay);
//         }
//     }
// }