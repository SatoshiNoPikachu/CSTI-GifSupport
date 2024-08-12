using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GifSupport;

/// <summary>
/// GIF播放器
/// </summary>
[DisallowMultipleComponent]
public class GifPlayer : MonoBehaviour
{
    /// <summary>
    /// 是否循环播放
    /// </summary>
    public bool Loop { get; set; }

    /// <summary>
    /// 播放状态
    /// </summary>
    public PlayStatus Status { get; private set; } = PlayStatus.End;

    /// <summary>
    /// Image组件
    /// </summary>
    public Image Image { get; private set; }

    /// <summary>
    /// Gif对象
    /// </summary>
    public Gif Gif { get; private set; }

    /// <summary>
    /// 当前帧数
    /// </summary>
    private int _current;

    private void Awake()
    {
        if (GetComponents<GifPlayer>().Length > 1)
        {
            Debug.LogWarning("There is already an instance of GifPlayer on this GameObject. Destroying this instance.");
            Destroy(this);
            return;
        }

        Image = GetComponent<Image>();
        if (Image) return;
        Destroy(this);
    }

    private void OnEnable()
    {
        if (Gif is null) return;
        if (Status is PlayStatus.Idle) StartPlay();
    }

    private void OnDisable()
    {
        if (Status is not PlayStatus.End) StopPlay();
    }

    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="gif">Gif对象</param>
    /// <param name="loop">是否循环播放</param>
    public void Setup(Gif gif, bool loop = true)
    {
        StopPlay();

        if (gif is null) return;
        Gif = gif;
        Loop = loop;
        Status = PlayStatus.Idle;
        // Image.sprite = null;
        // Image.overrideSprite = null;

        StartPlay(true);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <returns></returns>
    private IEnumerator Play()
    {
        while (true)
        {
            if (Status is not PlayStatus.Play) yield break;

            var frame = Gif[_current];
            Image.sprite = frame.Image;

            _current = (_current + 1) % Gif.Frames.Count;
            if (_current == 0 && !Loop)
            {
                Status = PlayStatus.End;
                yield break;
            }

            yield return new WaitForSeconds(frame.Delay);
        }
    }

    /// <summary>
    /// 开始播放，如果播放正在进行则不会执行
    /// </summary>
    /// <param name="reset">是否重置到第一帧</param>
    private void StartPlay(bool reset = false)
    {
        if (!gameObject.activeInHierarchy || !enabled) return;
        if (Status is PlayStatus.Play) return;
        if (reset) _current = 0;
        Status = PlayStatus.Play;
        StartCoroutine(Play());
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    private void StopPlay()
    {
        StopAllCoroutines();
        Status = PlayStatus.Idle;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        StopAllCoroutines();
        Gif = null;
        Loop = false;
        Status = PlayStatus.End;
    }

    /// <summary>
    /// 播放状态
    /// </summary>
    public enum PlayStatus
    {
        /// <summary>
        /// 播放空闲
        /// </summary>
        Idle,

        /// <summary>
        /// 正在播放
        /// </summary>
        Play,

        /// <summary>
        /// 播放结束
        /// </summary>
        End,
    }
}