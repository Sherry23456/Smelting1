using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Joystick Settings")]
    public RectTransform background; // 摇杆背景
    public RectTransform handle;    // 摇杆手柄
    public float handleRange = 1f; // 手柄移动范围

    private Vector2 input = Vector2.zero;

    // 公开摇杆输入值
    public Vector2 Input => input;

    void Start()
    {
        // 初始化手柄位置
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    // 当拖动摇杆时调用
    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // 计算摇杆输入
        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, background.position);
        Vector2 radius = background.sizeDelta / 2;
        input = (eventData.position - position) / (radius * handleRange);
        input = Vector2.ClampMagnitude(input, 1f);

        // 移动手柄
        handle.anchoredPosition = input * radius * handleRange;
    }

    // 当点击摇杆时调用
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    // 当释放摇杆时调用
    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}