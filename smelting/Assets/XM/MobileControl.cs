using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class MobileControl : MonoBehaviour
{
    [Header("摇杆 UI")]
    public RectTransform joystickBg;
    public RectTransform joystickHandle;

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float moveSmooth = 12f;

    [Header("转向设置")]
    public float lookSensitivity = 2.2f;
    public float lookSmooth = 10f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("摇杆")]
    public float joystickRadius = 60f;

    private CharacterController cc;
    private Transform mainCam;
  

    private Vector2 moveInput;
    private Vector2 smoothMove;
    private readonly Vector3 localOffset = new Vector3(0, 0, 0f);

    private Vector2 lookInput;
    private Vector2 smoothLook;
    private float pitch;

    private int moveFingerId = -1;
    private int lookFingerId = -1;

    
    void Awake()
    {
        cc = GetComponent<CharacterController>();
        mainCam = Camera.main.transform;
    }

    void Update()
    {
        HandleTouchInput();
        Move();
        Look();
        
    }

    // 核心：严格左右屏分区触摸
    void HandleTouchInput()
    {
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;

        foreach (var touch in Input.touches)
        {
            // 左手区域：屏幕左半边
            if (touch.position.x < Screen.width * 0.5f)
            {
                // 只处理移动摇杆，不处理转向
                if (touch.phase == TouchPhase.Began)
                {
                    if (moveFingerId == -1)
                    {
                        moveFingerId = touch.fingerId;
                        joystickHandle.position = touch.position; // 摇杆出现在手指位置
                    }
                }

                if (touch.fingerId == moveFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        Vector2 dir = touch.position - (Vector2)joystickBg.position;
                        dir = Vector2.ClampMagnitude(dir, joystickRadius);
                        moveInput = dir / joystickRadius;
                        joystickHandle.anchoredPosition = dir;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        moveFingerId = -1;
                        moveInput = Vector2.zero;
                        joystickHandle.anchoredPosition = Vector2.zero;
                    }
                }
            }
            // 右手区域：屏幕右半边 → 只控制转向
            else
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (lookFingerId == -1)
                        lookFingerId = touch.fingerId;
                }

                if (touch.fingerId == lookFingerId)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        lookInput = touch.deltaPosition;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        lookFingerId = -1;
                    }
                }
            }



        }
    }


    

    void Move()
    {
        smoothMove = Vector2.Lerp(smoothMove, moveInput, Time.deltaTime * moveSmooth);

        Vector3 f = mainCam.forward;
        Vector3 r = mainCam.right;
        f.y = 0; r.y = 0;
        f.Normalize(); r.Normalize();

        Vector3 dir = f * smoothMove.y + r * smoothMove.x;
        cc.Move(dir * moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        smoothLook = Vector2.Lerp(smoothLook, lookInput, Time.deltaTime * lookSmooth);

        // 左右旋转角色
        transform.Rotate(0, smoothLook.x * lookSensitivity * Time.deltaTime, 0);

        // 上下旋转相机
        pitch -= smoothLook.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        mainCam.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    


}
