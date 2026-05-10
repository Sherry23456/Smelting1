using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class TimelineSubtitleOnly2 : MonoBehaviour
{
    [Header("关闭字幕按钮")]
    public Button closeSubBtn;

    private TextMeshProUGUI _tmpText;
    private bool isPlaying = false;
    private Sequence _seq;

    // 原文均匀分句，每句长度接近
    private string[] sentences =
    {
        "在中国古代冶炼发展历程中，涌现出多位技艺卓越的先驱人物。",
        "春秋欧冶子是铸剑宗师，精通青铜与钢铁锻造，铸就传世名剑，开创吴越铸剑工艺。",
        "南北朝綦毋怀文完善灌钢技法，创新淬火工艺，大幅提升古代炼钢水平。",
        "北宋张潜总结推广胆水浸铜法，开创湿法炼铜规模化应用。",
        "沈括在《梦溪笔谈》中详实记载矿产冶金与炼铜原理，留存珍贵科技史料。",
        "明代宋应星著《天工开物》，系统梳理采矿、冶铁、铸器全套工艺。",
        "集古代冶炼技术之大成，传承千年冶金智慧。",
        "一代代匠人学者薪火相传，共同铸就了中华古代冶炼文明的辉煌成就。"
    };

    // 配音总长改为51秒
    private readonly float totalVoiceTime = 51f;

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

        SafeKillTween();
        _seq = DOTween.Sequence();
        _seq.SetId(this);

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
            .OnComplete(() => isPlaying = false);
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