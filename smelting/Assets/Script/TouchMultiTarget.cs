using UnityEngine;
using UnityEngine.Video;

public class TouchMultiTarget : MonoBehaviour
{
    public Camera cam;

    [Header("可点击的目标物体")]
    public GameObject[] targetObjects;

    [Header("点击后显示的UI")]
    public GameObject[] uiimage;

    [Header("摇杆")]
    public GameObject joystick;

    [Header("点击检测参数")]
    [Tooltip("允许的最大移动像素（超过则判定为滑动）")]
    public float maxMoveDistance = 20f;

    // 记录触摸起始位置
    private Vector2 touchStartPos;
    //视频
    private VideoPlayer v1, v2;
    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        // 没有触摸直接返回
        if (Input.touchCount <= 0)
            return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            // 手指刚按下 → 记录起点
            case TouchPhase.Began:
                touchStartPos = touch.position;
                break;

            // 手指抬起 → 判断是否是有效点击
            case TouchPhase.Ended:
                HandleTouchClick(touch);
                break;
        }
    }

    /// <summary>
    /// 处理有效点击（只有不滑动才算点击）
    /// </summary>
    void HandleTouchClick(Touch touch)
    {
        // 计算触摸移动距离
        float moveDelta = Vector2.Distance(touchStartPos, touch.position);

        // 如果滑动距离超过阈值 → 判定为旋转视角，不触发点击
        if (moveDelta > maxMoveDistance)
            return;

        // 发射射线检测
        Ray ray = cam.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 检测是否是目标物体
            if (System.Array.IndexOf(targetObjects, hit.collider.gameObject) != -1)
            {
                Debug.Log("有效点击：" + hit.collider.name);
                OnHitTarget(hit.collider.gameObject);
            }
        }
    }

    void OnHitTarget(GameObject obj)
    {
        switch (obj.name)
        {
            case "tieli":
                uiimage[0].SetActive(true);
                cam.GetComponent<TouchCameraController2>().enabled = false;
                joystick.SetActive(false);
                break;
            case "shipin01":
                v1 = targetObjects[3].GetComponent<VideoPlayer>();
                if (v1.isPlaying)
                {
                    v1.Pause();
                }
                else
                {
                    v1.Play();
                }
                break;
            default:
                break;
        }
    }
}