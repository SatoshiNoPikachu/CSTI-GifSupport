using System;
using GifSupport.Data;
using UnityEngine.UI;

namespace GifSupport.CstiGif;

[Serializable]
public class GifPlaySet
{
    /// <summary>
    /// GIF对象
    /// </summary>
    public Gif Gif => Database.GetData<Gif>(GifName);

    /// <summary>
    /// GIF名称
    /// </summary>
    public string GifName;

    /// <summary>
    /// 是否循环播放
    /// </summary>
    public bool Loop;

    /// <summary>
    /// 应用GIF设置
    /// </summary>
    /// <param name="player">播放器</param>
    public void Apply(GifPlayer player)
    {
        if (Gif is null || player is null) return;
        if (player.Gif == Gif) return;

        player.Setup(Gif, Loop);
    }

    /// <summary>
    /// 应用GIF设置，会自动添加播放器组件并清除图像组件的覆盖精灵
    /// </summary>
    /// <param name="image">图像组件</param>
    public void Apply(Image image)
    {
        if (!image) return;
        var player = image.gameObject.GetComponent<GifPlayer>();
        if (!player) player = image.gameObject.AddComponent<GifPlayer>();
        
        image.overrideSprite = null;
        Apply(player);
    }
}