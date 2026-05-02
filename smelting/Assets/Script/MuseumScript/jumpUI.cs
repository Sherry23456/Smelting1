using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 记得引入DOTween的命名空间

public class jumpUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform buttonPanel; // 需要弹出/收回的按钮面板
    public Button toggleButton;       // 右上角的触发按钮

    [Header("Animation Settings")]
    public float animationDuration = 0.3f; // 动画持续时间
    public float hiddenX = 200f;           // 隐藏时的X轴坐标（右侧外）
    public float shownX = 0f;              // 显示时的X轴坐标

    private bool isExpanded = false;       // 记录面板当前状态
    private Tween currentTween;            // 用于存储当前动画

    void Start()
    {
        // 为按钮添加点击事件监听
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleMenu);

        // 确保面板起始状态为隐藏
        if (buttonPanel != null)
        {
            Vector2 startPos = buttonPanel.anchoredPosition;
            startPos.x = hiddenX;
            buttonPanel.anchoredPosition = startPos;
        }
    }

    // 按钮点击时调用此方法
    public void ToggleMenu()
    {
        // 如果已有动画在播放，先把它停掉，防止冲突
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        // 计算目标位置的X坐标
        float targetX = isExpanded ? hiddenX : shownX;

        // 播放位移动画，并设置缓动效果
        currentTween = buttonPanel.DOAnchorPosX(targetX, animationDuration)
                                   .SetEase(Ease.OutQuad); // 缓动曲线，让动画更柔和

        // 切换状态
        isExpanded = !isExpanded;
    }
}