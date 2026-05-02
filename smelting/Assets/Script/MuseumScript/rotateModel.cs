using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 功能：UI的RawImage上渲染3D模型，只在RawImage上旋转，不影响按钮点击
/// 解决：按钮和模型旋转区域冲突问题
/// </summary>
public class rotateModel : MonoBehaviour
{
    [Header("需要旋转的模型Transform")]
    public Transform modelTransform;

    [Header("旋转灵敏度")]
    public float rotateScale = 1f;

    [Header("显示模型的RawImage(必须拖入)")]
    public RawImage modelRawImage;

    // 是否正在旋转中
    private bool isRotate;
    // 记录开始触摸/鼠标按下的位置
    private Vector2 startMousePosition;
    // 记录模型开始旋转前的欧拉角
    private Vector3 startModelRotation;
    // 触屏变量
    private Touch touch;

    void Update()
    {
        // ==============================================
        // 核心判断：只有鼠标/手指在【模型RawImage】上，才允许旋转
        // 如果不在RawImage上（比如在按钮上），直接不执行旋转
        // ==============================================
        if (!IsPointerOverModelImage())
        {
            isRotate = false;
            return;
        }

        // ==============================================
        // 安卓 单指触摸旋转逻辑
        // ==============================================
        if (Input.touchCount == 1)
        {
            touch = Input.GetTouch(0);

            // 手指刚按下：记录起始位置和模型角度
            if (touch.phase == TouchPhase.Began && !isRotate)
            {
                isRotate = true;
                startMousePosition = touch.position;
                startModelRotation = modelTransform.eulerAngles;
            }

            // 手指抬起/取消：结束旋转
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isRotate = false;
            }

            // 手指滑动中：计算偏移量 → 旋转模型
            if (isRotate && touch.phase == TouchPhase.Moved)
            {
                // 计算当前位置与起始位置的偏移
                Vector2 mouseDelta = touch.position - startMousePosition;

                // Y偏移控制上下旋转(X轴)，X偏移控制左右旋转(Y轴)，取反保证方向正常
                Vector3 rotation = new Vector3(mouseDelta.y, -mouseDelta.x, 0) * rotateScale;

                // 应用旋转
                modelTransform.eulerAngles = startModelRotation + rotation;
            }
        }
        // ==============================================
        // PC 鼠标旋转逻辑（和触屏完全一致）
        // ==============================================
        else
        {
            // 鼠标左键按下：记录起始状态
            if (Input.GetMouseButtonDown(0) && !isRotate)
            {
                isRotate = true;
                startMousePosition = Input.mousePosition;
                startModelRotation = modelTransform.eulerAngles;
            }

            // 鼠标抬起：结束旋转
            if (Input.GetMouseButtonUp(0))
            {
                isRotate = false;
            }

            // 按住鼠标拖动：旋转模型
            if (isRotate)
            {
                Vector2 mouseDelta = (Vector2)Input.mousePosition - startMousePosition;
                Vector3 rotation = new Vector3(mouseDelta.y, -mouseDelta.x, 0) * rotateScale;
                modelTransform.eulerAngles = startModelRotation + rotation;
            }
        }
    }

    // ==============================================
    // 核心功能：判断 当前鼠标/手指 是否 在 modelRawImage 上
    // 返回 true = 在模型区域 → 可以旋转
    // 返回 false = 在按钮/其他UI → 不旋转，按钮正常点击
    // ==============================================
    private bool IsPointerOverModelImage()
    {
        // 没拖RawImage直接返回false
        if (modelRawImage == null) return false;

        // 创建UI射线检测数据
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        // 设置射线起点为当前触摸/鼠标位置
        if (Input.touchCount == 1)
            eventData.position = Input.GetTouch(0).position;
        else
            eventData.position = Input.mousePosition;

        // 射线检测所有UI
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 遍历检测结果，判断是否点中了 modelRawImage
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == modelRawImage.gameObject)
            {
                return true;
            }
        }

        // 没点中模型区域
        return false;
    }
}