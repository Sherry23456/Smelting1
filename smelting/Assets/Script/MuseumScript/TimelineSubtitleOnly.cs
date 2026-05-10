using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class TimelineSubtitleOnly : MonoBehaviour
{
    [Header("关闭字幕按钮")]
    public Button closeSubBtn;

    private TextMeshProUGUI _tmpText;
    private bool isPlaying = false;
    private Sequence _seq;

    // 你的最新均匀分句文案
    private string[] sentences =
    {
        "中国古代冶炼技术，是中华先民智慧的结晶。",
        "从新石器时代的简单烧陶炼铜，到春秋战国的铁器革新。",
        "再到后世的冶炼成熟，每一步都推动着农业手工业军事的发展。",
        "成为中华民族延续的重要支撑。",
        "其中马家窑文化青铜刀，是我国早期冶铜代表。",
        "夏代二里头青铜爵见证了远古青铜文明萌芽。",
        "春秋战国青铜柄铁剑，实现了铜铁合铸的革新。",
        "汉代铁犁范推进了铁器农具的普及，而后世的章丘铁锅则是传统冶炼技艺的生动传承。"
    };

    private readonly float totalVoiceTime = 49f;

    void Awake()
    {
        _tmpText = GetComponent<TextMeshProUGUI>();
        if (closeSubBtn != null)
            closeSubBtn.onClick.AddListener(CloseSubtitle);

        _tmpText.text = "";
    }

    void OnEnable()
    {
        if (isPlaying) return;
        PlayEqualSentenceSubtitle();
    }

    void PlayEqualSentenceSubtitle()
    {
        isPlaying = true;
        _tmpText.text = "";

        // 安全清理
        if (_seq != null) _seq.Kill();
        DOTween.Kill(this);

        _seq = DOTween.Sequence();
        _seq.SetId(this); // 给动画加ID，安全销毁

        float perSentenceTime = totalVoiceTime / sentences.Length;

        foreach (string sen in sentences)
        {
            _seq.AppendCallback(() =>
            {
                if (_tmpText == null) return;
                _tmpText.text = sen;
            });
            _seq.AppendInterval(perSentenceTime);
        }

        _seq.SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isPlaying = false;
            });
    }

    public void CloseSubtitle()
    {
        SafeKillTween();
        _tmpText.text = "";
        isPlaying = false;
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        SafeKillTween();
        isPlaying = false;
    }

    void OnDestroy()
    {
        SafeKillTween();
    }

    // 安全清理动画（解决报错核心）
    void SafeKillTween()
    {
        if (_seq != null)
        {
            _seq.Kill();
            _seq = null;
        }
        DOTween.Kill(this);
    }
}