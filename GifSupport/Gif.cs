using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGifDecoder;

namespace GifSupport;

public class Gif
{
    // public string Name { get; private set; } = name;

    public List<Frame> Frames { get; } = [];

    /// <summary>
    /// 加载GIF
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static Gif LoadGif(string path, string name)
    {
        try
        {
            var gif = new Gif();
            var index = 0;
            using var stream = new GifStream(path);
            while (stream.HasMoreData)
            {
                switch (stream.CurrentToken)
                {
                    case GifStream.Token.Image:
                        var image = stream.ReadImage();
                        var tex = new Texture2D(stream.Header.width, stream.Header.height, TextureFormat.ARGB32, false);
                        tex.SetPixels32(image.colors);
                        tex.Apply(true, true);

                        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                        var frame = new Frame(sprite, image.DelaySeconds);
                        gif.AddFrame(frame);

                        var objName = $"{name}_{index:00}";
                        tex.name = objName;
                        sprite.name = objName;
                        index++;
                        break;

                    case GifStream.Token.Comment:
                    // var commentText = stream.ReadComment();
                    // Plugin.Log.LogInfo(commentText);
                    // break;
                    case GifStream.Token.Header:
                    case GifStream.Token.Palette:
                    case GifStream.Token.GraphicsControl:
                    case GifStream.Token.ImageDescriptor:
                    case GifStream.Token.PlainText:
                    case GifStream.Token.NetscapeExtension:
                    case GifStream.Token.ApplicationExtension:
                    case GifStream.Token.EndOfFile:
                    default:
                        stream.SkipToken();
                        break;
                }
            }

            return gif;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 帧索引器
    /// </summary>
    /// <param name="index">帧索引</param>
    public Frame this[int index] => Frames[index];

    /// <summary>
    /// 添加帧
    /// </summary>
    /// <param name="frame">帧</param>
    private void AddFrame(Frame frame)
    {
        if (frame is null) return;
        Frames.Add(frame);
    }

    /// <summary>
    /// 帧
    /// </summary>
    /// <param name="sprite">精灵</param>
    /// <param name="delay">延迟</param>
    public class Frame(Sprite sprite, float delay)
    {
        public Sprite Image { get; private set; } = sprite;
        public float Delay { get; private set; } = delay;
    }
}