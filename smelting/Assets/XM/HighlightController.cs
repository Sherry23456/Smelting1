using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HighlightController : MonoBehaviour
{
    private Material highlightMat;
    private Renderer rend;

    [Header("高亮控制")]
    public Color outlineColor = new Color(0, 1, 1, 1);
    public float outlineWidth = 0.03f;
    public float outlineIntensity = 2.5f;

    [Header("呼吸效果")]
    public bool enableBreath = true;
    public float breathSpeed = 3.0f;
    public float breathAmplitude = 0.4f;
    public float breathMinScale = 0.75f;
    public Color breathColorShift = new Color(0.3f, 0.1f, 0.3f, 0);

    void Start()
    {
        rend = GetComponent<Renderer>();
        // 复制材质，避免影响其他相同材质的物体
        highlightMat = new Material(Shader.Find("Custom/HighlightOutline"));
        rend.material = highlightMat;
    }

    void Update()
    {
        if (highlightMat == null) return;

        // 实时更新所有参数（支持运行时调节）
        highlightMat.SetColor("_OutlineColor", outlineColor);
        highlightMat.SetFloat("_OutlineWidth", outlineWidth);
        highlightMat.SetFloat("_OutlineIntensity", outlineIntensity);

        highlightMat.SetFloat("_BreathSpeed", enableBreath ? breathSpeed : 0);
        highlightMat.SetFloat("_BreathAmplitude", enableBreath ? breathAmplitude : 0);
        highlightMat.SetFloat("_BreathMinScale", breathMinScale);
        highlightMat.SetColor("_BreathColorShift", breathColorShift);
    }

    // 外部调用：开启/关闭高亮
    public void SetHighlight(bool active, float duration = 0f)
    {
        enabled = active;
        if (duration > 0)
        {
            Invoke(nameof(DisableHighlight), duration);
        }
    }

    void DisableHighlight()
    {
        enabled = false;
    }

    // 示例：动态修改颜色（比如根据敌人类型）
    public void ChangeOutlineColor(Color newColor)
    {
        outlineColor = newColor;
    }
}