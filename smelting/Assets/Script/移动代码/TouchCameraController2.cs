using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchCameraController2 : MonoBehaviour
{
    [Header("相机模式")]
    [SerializeField] private CameraMode cameraMode = CameraMode.Orbit;

    [Header("轨道相机设置（围绕目标旋转）")]
    [SerializeField] private Transform orbitTarget;
    [SerializeField] private float orbitDistance = 10f;
    [SerializeField] private float orbitSpeed = 0.5f;

    [Header("自由相机设置（自由旋转）")]
    [SerializeField] private float freeRotateSpeed = 0.3f;

    [Header("通用设置")]
    [SerializeField] private bool invertY = true;
    [SerializeField] private bool invertX = false;
    [SerializeField] private float smoothness = 5f;

    [Header("缩放设置")]
    [SerializeField] private bool enablePinchZoom = true;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("触摸区域设置")]
    [SerializeField] private float rightAreaStartRatio = 0.4f; // 右侧区域起始位置
    [SerializeField] private bool ignoreUIElements = true; // 是否忽略UI元素
    [SerializeField] private bool debugMode = false;

    [Header("引用")]
    public VirtualJoystick joystick; // 引用摇杆（可选）

    private enum CameraMode
    {
        Orbit,      // 轨道相机（围绕目标旋转）
        Free        // 自由相机（自由旋转）
    }

    private Vector2 lastTouchPosition;
    private bool isRotating = false;
    private float currentDistance;
    private float targetXRotation;
    private float targetYRotation;
    private float currentXRotation;
    private float currentYRotation;

    // 用于检测是否点击在UI上的组件
    private EventSystem eventSystem;

    void Start()
    {
        // 获取EventSystem
        eventSystem = EventSystem.current;

        // 初始化
        if (cameraMode == CameraMode.Orbit && orbitTarget == null)
        {
            Debug.LogWarning("未设置轨道目标，创建一个默认目标");
            GameObject target = new GameObject("CameraTarget");
            target.transform.position = Vector3.zero;
            orbitTarget = target.transform;
        }

        // 设置初始位置和旋转
        if (cameraMode == CameraMode.Orbit && orbitTarget != null)
        {
            Vector3 direction = (transform.position - orbitTarget.position).normalized;
            currentDistance = Vector3.Distance(transform.position, orbitTarget.position);

            Vector3 angles = transform.eulerAngles;
            targetXRotation = angles.x;
            targetYRotation = angles.y;
        }
        else
        {
            Vector3 angles = transform.eulerAngles;
            targetXRotation = angles.x;
            targetYRotation = angles.y;
        }

        currentXRotation = targetXRotation;
        currentYRotation = targetYRotation;
        currentDistance = orbitDistance;
    }

    void Update()
    {
        // 处理旋转输入
        HandleRotationInput();

        if (enablePinchZoom)
        {
            HandlePinchZoom();
        }

        // 平滑更新相机
        SmoothUpdateCamera();
    }

    // 检查触摸是否在UI元素上
    private bool IsPointerOverUIObject(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    // 检查触摸是否在摇杆上
    private bool IsPointerOverJoystick(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // 检查是否是摇杆组件
            if (result.gameObject.GetComponent<VirtualJoystick>() != null)
                return true;

            // 检查是否是摇杆的子物体
            if (result.gameObject.GetComponentInParent<VirtualJoystick>() != null)
                return true;
        }

        return false;
    }

    // 检查是否在相机控制区域
    private bool IsInCameraControlArea(Vector2 screenPosition)
    {
        // 如果在UI上且启用了忽略UI，则不处理
        if (ignoreUIElements && IsPointerOverUIObject(screenPosition))
        {
            if (debugMode)
                Debug.Log("触摸在UI上，不处理相机控制");
            return false;
        }

        // 检查是否在右侧区域
        bool isRightArea = screenPosition.x > Screen.width * rightAreaStartRatio;

        if (debugMode && isRightArea)
            Debug.Log($"触摸在右侧区域: {screenPosition.x} > {Screen.width * rightAreaStartRatio}");

        return isRightArea;
    }

    void HandleRotationInput()
    {
        // 单指旋转
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 screenPos = touch.position;
            if (screenPos.x > Screen.width / 4)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        lastTouchPosition = touch.position;
                        isRotating = true;
                        if (debugMode) Debug.Log("开始旋转");
                        break;

                    case TouchPhase.Moved:
                        if (isRotating)
                        {
                            Vector2 touchDelta = touch.position - lastTouchPosition;

                            // 计算旋转增量
                            float rotX = touchDelta.y * (cameraMode == CameraMode.Orbit ? orbitSpeed : freeRotateSpeed) * (invertY ? -1 : 1);
                            float rotY = touchDelta.x * (cameraMode == CameraMode.Orbit ? orbitSpeed : freeRotateSpeed) * (invertX ? 1 : -1);

                            // 更新目标旋转
                            targetXRotation += rotX;
                            targetYRotation += rotY;

                            // 限制垂直旋转角度
                            targetXRotation = Mathf.Clamp(targetXRotation, -80f, 80f);

                            lastTouchPosition = touch.position;

                            if (debugMode)
                                Debug.Log($"旋转中: rotX={rotX}, rotY={rotY}, targetXRotation={targetXRotation}");
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isRotating = false;
                        if (debugMode) Debug.Log("结束旋转");
                        break;
                }
            }

        }
        else if (Input.touchCount > 1)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            Vector2 screenPos1 = touch1.position;
            Vector2 screenPos2 = touch2.position;
          
            // 多点触控时暂停旋转
            isRotating = false;
        }
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            Vector2 screenPos1 = touch1.position;
            Vector2 screenPos2 = touch2.position;

            // 检查两个触摸点是否都在相机控制区域
            if (IsInCameraControlArea(screenPos1) && IsInCameraControlArea(screenPos2))
            {
                // 获取前一帧的两点距离
                Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
                Vector2 prevPos2 = touch2.position - touch2.deltaPosition;
                float prevDistance = Vector2.Distance(prevPos1, prevPos2);

                // 获取当前帧的两点距离
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                // 计算距离变化
                float deltaDistance = currentDistance - prevDistance;

                // 应用缩放
                if (cameraMode == CameraMode.Orbit && orbitTarget != null)
                {
                    orbitDistance -= deltaDistance * zoomSpeed;
                    orbitDistance = Mathf.Clamp(orbitDistance, minZoom, maxZoom);

                    if (debugMode)
                        Debug.Log($"缩放: delta={deltaDistance}, orbitDistance={orbitDistance}");
                }
                else
                {
                    // 自由相机模式的缩放
                    transform.Translate(Vector3.forward * deltaDistance * zoomSpeed, Space.Self);
                }
            }
        }
    }

    void SmoothUpdateCamera()
    {
        // 平滑插值旋转
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, Time.deltaTime * smoothness);
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * smoothness);

        if (cameraMode == CameraMode.Orbit && orbitTarget != null)
        {
            // 轨道相机模式
            Quaternion rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);

            // 计算相机位置（围绕目标）
            Vector3 desiredPosition = orbitTarget.position + rotation * (Vector3.back * orbitDistance);

            // 更新相机位置和旋转
            transform.position = desiredPosition;
            transform.LookAt(orbitTarget);
        }
        else
        {
            // 自由相机模式
            transform.rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
        }
    }

    // 公开方法：设置轨道目标
    public void SetOrbitTarget(Transform newTarget)
    {
        orbitTarget = newTarget;
        if (orbitTarget != null)
        {
            Vector3 directionToTarget = orbitTarget.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            Vector3 angles = lookRotation.eulerAngles;
            targetXRotation = angles.x;
            targetYRotation = angles.y;

            orbitDistance = directionToTarget.magnitude;
        }
    }
}