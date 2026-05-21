using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FadeEffect : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private float fadeDuration = 1f;      // 渐变持续时间（秒）
    [SerializeField] private Color blackColor = Color.black; // 黑色

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

   void OnEnable()
    {
        StartCoroutine(Fade());
    }

    private IEnumerator Fade() {
        FlashBlack();
        yield return new WaitForSeconds(3);

        FadeInOut();

    }
    /// <summary>
    /// 黑屏消失（变透明）
    /// </summary>
    public void FadeInOut()
    {
        canvasGroup.DOFade(0f, 0.5f);
    }

    /// <summary>
    /// 从透明到黑色再返回
    /// </summary>
    public void FlashBlack()
    {
        canvasGroup.DOFade(1f, 1f);
           
    }
}