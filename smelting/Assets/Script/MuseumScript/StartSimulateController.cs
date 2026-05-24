using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSimulateController : MonoBehaviour
{
    [Header("黑屏 Image")]
    public Image blackImage;

    [Header("提示文本 TMP")]
    public TextMeshProUGUI tipsText;

    [Header("启动按钮")]
    public Button startBtn;

    [Header("旁白语音 (40秒)")]
    public AudioClip voiceClip;

    private Sequence _seq;
    private AudioSource audioSource;

    void Awake()
    {
        // 直接获取你手动添加的 AudioSource
        audioSource = GetComponent<AudioSource>();

        blackImage.gameObject.SetActive(false);
        tipsText.gameObject.SetActive(false);

        if (startBtn != null)
            startBtn.onClick.AddListener(StartAnimation);
    }

    void StartAnimation()
    {
        startBtn.interactable = false;

        // 1. 黑屏淡入
        blackImage.gameObject.SetActive(true);
        blackImage.color = new Color(0, 0, 0, 0);
        blackImage.DOFade(1, 0.5f);

        _seq = DOTween.Sequence();
        _seq.AppendInterval(0.6f);

        // 2. 显示：即将进入模拟
        _seq.AppendCallback(() =>
        {
            tipsText.gameObject.SetActive(true);
            tipsText.alpha = 0;
            tipsText.text = "即将进入模拟......";
        });
        _seq.Append(tipsText.DOFade(1, 0.5f));
        _seq.AppendInterval(2f);

        // 长文本
        string fullText = "本次模拟展示的是中国古代先进的灌钢冶炼技术。早在南北朝时期，冶金家綦毋怀文改良并完善了这一经典炼钢工艺。古人率先利用水力水锤粉碎矿石，大幅提升矿石利用率，再通过高温高炉熔炼矿石，浇铸得到粗铁坯。最后采用双生铁包裹熟铁的独特合炼方式，让生铁的高碳与熟铁的韧性相互融合，去除杂质、优化材质，最终锻打出质地坚硬、性能优良的精钢，充分展现了中国古代领先世界的冶金智慧与工艺创新。";

        // 3. 开始显示长文本 + 同步播放语音
        _seq.AppendCallback(() =>
        {
            tipsText.alpha = 1;
            tipsText.text = fullText;
            tipsText.maxVisibleCharacters = 0;

            // 同步播放语音
            if (audioSource != null && voiceClip != null)
            {
                audioSource.clip = voiceClip;
                audioSource.Play();
            }
        });

        // 逐字显示 40 秒（和语音完全同步）
        _seq.Append(DOTween.To(
            () => tipsText.maxVisibleCharacters,
            x => tipsText.maxVisibleCharacters = x,
            fullText.Length,
            40f
        ));

        // 语音播完停留
        _seq.AppendInterval(2f);

        // 文字消失
        _seq.Append(tipsText.DOFade(0, 1f));
        _seq.AppendInterval(0.5f);

        // 4. 显示进入模拟
        _seq.AppendCallback(() =>
        {
            tipsText.text = "进入模拟";
            tipsText.alpha = 1;
        });

        _seq.AppendInterval(2f);

        // 5. 加载场景
        _seq.AppendCallback(() =>
        {
            if (audioSource != null) audioSource.Stop();
            SceneManager.LoadScene(2);
        });

        _seq.SetLink(gameObject);
    }

    void OnDestroy()
    {
        if (_seq != null) _seq.Kill();
        if (audioSource != null) audioSource.Stop();
    }
}