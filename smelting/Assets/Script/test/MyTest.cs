using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class MyTest : MonoBehaviour
{
    Button btn;
    Text txt;
    DateTime now;
    // Start is called before the first frame update
    void Start()
    {
        txt= GameObject.Find("Canvas"). transform.Find("Text-btn").GetComponent<Text>();
        btn = GameObject.Find("Canvas"). transform.Find("ButtonBrn").GetComponent<Button>();
        btn.onClick.AddListener(() => { txt.text = now.ToString(); });
        target = GameObject.Find("Cube").transform;

        // 初始化当前旋转角度
        Vector3 angles = transform.eulerAngles;
        currentXRotation = angles.x;
        currentYRotation = angles.y;
    }
    Ray ray;
    RaycastHit hit;
    // Update is called once per frame
    void Update()
    {
        now = DateTime.Now;
       

        //触屏
        HandleTouchInput();

        // 检查是否有触摸输入
        if (Input.touchCount > 0)
        {
            // 获取第一个手指的触摸信息（索引0）
            Touch touch = Input.GetTouch(0);

            // 根据触摸阶段处理不同逻辑
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
                    break;

                case TouchPhase.Moved:
                    Debug.Log("手指移动 - 位置：" + touch.position);
                    Debug.Log("移动距离：" + touch.deltaPosition);
                    // 处理触摸移动的逻辑
                    break;

                case TouchPhase.Stationary:
                    Debug.Log("手指静止不动");
                    // 处理手指静止的逻辑
                    break;

                case TouchPhase.Ended:
                    Debug.Log("手指离开屏幕");
                    // 处理触摸结束的逻辑
                    break;

                case TouchPhase.Canceled:
                    Debug.Log("触摸被取消");
                    // 处理触摸被取消的逻辑
                    break;
            }

            // 获取触摸的其他信息
            Debug.Log("触摸压力：" + touch.pressure); // 支持压力感应的设备
            Debug.Log("触摸ID：" + touch.fingerId);   // 用于区分不同手指
        }

    }
    [Header("相机设置")]
    [SerializeField] private Transform target;           // 相机围绕的目标对象
    [SerializeField] private float rotationSpeed = 10f; // 旋转速度
    [SerializeField] private bool invertY = false;       // 反转Y轴

    [Header("旋转限制")]
    [SerializeField] private bool limitXRotation = true; // 限制X轴旋转
    [SerializeField] private float minXAngle = -80f;     // 最小X角度
    [SerializeField] private float maxXAngle = 80f;      // 最大X角度

    private Vector2 lastTouchPosition;
    private bool isTouching = false;
    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    void HandleTouchInput()
    {
        // 检查是否有触摸输入
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // 开始触摸，记录初始位置
                    lastTouchPosition = touch.position;
                    isTouching = true;
                    break;

                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        // 计算触摸移动的增量
                        Vector2 touchDelta = touch.position - lastTouchPosition;

                        // 应用旋转
                        RotateCamera(touchDelta);

                        // 更新最后位置
                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // 触摸结束
                    isTouching = false;
                    break;
            }
        }
        else
        {
            // 没有触摸时重置触摸状态
            isTouching = false;
        }
    }

    void RotateCamera(Vector2 delta)
    {
        // 计算旋转角度
        float rotationX = delta.y * rotationSpeed * (invertY ? -1 : 1);
        float rotationY = delta.x * rotationSpeed;

        // 应用旋转
        currentXRotation -= rotationX;
        currentYRotation += rotationY;

        // 限制X轴旋转角度
        if (limitXRotation)
        {
            currentXRotation = Mathf.Clamp(currentXRotation, minXAngle, maxXAngle);
        }

        // 应用旋转到相机
        Quaternion rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
        transform.rotation = rotation;

        // 计算相机位置（保持相机到目标的距离）
        if (target != null)
        {
            Vector3 direction = rotation * Vector3.back;
            float distance = Vector3.Distance(transform.position, target.position);
            transform.position = target.position + direction * distance;
        }
    }
}
