using System;
using UnityEngine;
using UnityEngine.UI;

public class TouchCameraController : MonoBehaviour
{
    Button btn;
    Text txt;
    DateTime now;
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
    [SerializeField] private float smoothness = 5f;  // 旋转平滑度

    [Header("缩放设置")]
    [SerializeField] private bool enablePinchZoom = true;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

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

    void Start()
    {
        txt = GameObject.Find("Canvas").transform.Find("Text-btn").GetComponent<Text>();
        btn = GameObject.Find("Canvas").transform.Find("ButtonBrn").GetComponent<Button>();
        btn.onClick.AddListener(() => { txt.text = now.ToString(); });
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
    Ray ray;
    RaycastHit hit;
    void Update()
    {
        now = DateTime.Now;
        // 处理旋转输入
        HandleRotationInput();

        // 处理缩放输入
        if (enablePinchZoom)
        {
            HandlePinchZoom();
        }

        // 平滑更新相机
        SmoothUpdateCamera();
    }

    void HandleRotationInput()
    {
        // 单指旋转
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {

                case TouchPhase.Began:
                    Debug.Log("手指开始触摸 - 位置：" + touch.position);
                    // 处理触摸开始的逻辑
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.name == "Cube")
                        {
                            txt.text = "手指开始触摸-这是一个3DDE正方体。";
                        }
                    }
                    lastTouchPosition = touch.position;
                    isRotating = true;
                    break;

                case TouchPhase.Moved:
                    if (isRotating)
                    {
                        Vector2 touchDelta = touch.position - lastTouchPosition;

                        // 计算旋转增量
                        float rotX = touchDelta.y * (cameraMode == CameraMode.Orbit ? orbitSpeed : freeRotateSpeed) * (invertY ? -1 : 1);
                        float rotY = touchDelta.x * (cameraMode == CameraMode.Orbit ? orbitSpeed : freeRotateSpeed) * (invertX ? -1 : 1);

                        // 更新目标旋转
                        targetXRotation += rotX;
                        targetYRotation += rotY;

                        // 限制垂直旋转角度
                        targetXRotation = Mathf.Clamp(targetXRotation, -80f, 80f);

                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isRotating = false;
                    break;
            }
        }
        else if (Input.touchCount > 1)
        {
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
            }
            else
            {
                // 自由相机模式的缩放可以改为移动相机位置
                transform.Translate(Vector3.forward * deltaDistance * zoomSpeed, Space.Self);
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
            // 重新计算旋转角度
            Vector3 directionToTarget = orbitTarget.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            Vector3 angles = lookRotation.eulerAngles;
            targetXRotation = angles.x;
            targetYRotation = angles.y;

            orbitDistance = directionToTarget.magnitude;
        }
    }
}