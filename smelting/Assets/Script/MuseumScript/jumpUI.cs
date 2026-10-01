using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class jumpUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform buttonPanel; 
    public Button toggleButton;    

    [Header("Animation Settings")]
    public float animationDuration = 0.3f; 
    public float hiddenX = 200f;      
    public float shownX = 0f;          

    private bool isExpanded = false;       
    private Tween currentTween;         

    void Start()
    {
       
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleMenu);

        if (buttonPanel != null)
        {
            Vector2 startPos = buttonPanel.anchoredPosition;
            startPos.x = hiddenX;
            buttonPanel.anchoredPosition = startPos;
        }
    }

   
    public void ToggleMenu()
    {
       
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        float targetX = isExpanded ? hiddenX : shownX;

       
        currentTween = buttonPanel.DOAnchorPosX(targetX, animationDuration)
                                   .SetEase(Ease.OutQuad); 

       
        isExpanded = !isExpanded;
    }
}