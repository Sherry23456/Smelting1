using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 文本闪烁（无警告版 · 修复OnDestroy清理问题）
/// </summary>
public class TextFlicker : MonoBehaviour
{
    [Header("基础设置")]
    public bool autoPlay = true;
    public bool loopFlicker = true;

    [Header("闪烁速度")]
    public float fadeInTime = 0.2f;
    public float fadeOutTime = 0.2f;
    public float visibleDelay = 0.1f;
    public float hiddenDelay = 0.1f;

    private Text uiText;
    private TextMeshProUGUI tmpText;
    private Sequence flickerSequence;

    void Awake()
    {
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TextMeshProUGUI>();

        if (uiText == null && tmpText == null)
        {
            Debug.LogError("未找到 Text 或 TextMeshProUGUI 组件！", gameObject);
            enabled = false;
        }
    }

    void Start()
    {
        if (autoPlay)
            StartFlicker();
    }

    // 关键修复：用 OnDisable 而不是 OnDestroy 清理动画
    void OnDisable()
    {
        StopFlicker();
    }

    [ContextMenu("开始闪烁")]
    public void StartFlicker()
    {
        StopFlicker();

        flickerSequence = DOTween.Sequence();

        if (tmpText != null)
            flickerSequence.Append(tmpText.DOFade(1f, fadeInTime));
        else
            flickerSequence.Append(uiText.DOFade(1f, fadeInTime));

        flickerSequence.AppendInterval(visibleDelay);

        if (tmpText != null)
            flickerSequence.Append(tmpText.DOFade(0f, fadeOutTime));
        else
            flickerSequence.Append(uiText.DOFade(0f, fadeOutTime));

        flickerSequence.AppendInterval(hiddenDelay);

        if (loopFlicker)
            flickerSequence.SetLoops(-1);

        flickerSequence.Play();
    }

    [ContextMenu("停止闪烁")]
    public void StopFlicker()
    {
        if (flickerSequence != null && flickerSequence.IsActive())
        {
            flickerSequence.Kill();
            flickerSequence = null;
        }

        SetTextAlpha(1);
    }

    private void SetTextAlpha(float alpha)
    {
        if (tmpText != null)
        {
            Color c = tmpText.color;
            c.a = alpha;
            tmpText.color = c;
        }
        else
        {
            Color c = uiText.color;
            c.a = alpha;
            uiText.color = c;
        }
    }
}