using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public VirtualJoystick rotationJoystick; // 右摇杆引用
    public float rotationSpeed = 100f;      // 旋转速度
    public float pitchMin = -80f;           // 相机上下旋转的最小角度
    public float pitchMax = 80f;            // 相机上下旋转的最大角度

    private Transform cameraTransform; // 相机的Transform组件

    void Start()
    {
        // 获取相机的Transform组件
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 如果右摇杆有输入，则旋转相机
        if (rotationJoystick != null && rotationJoystick.Input != Vector2.zero)
        {
            RotateCamera(rotationJoystick.Input);
        }
    }

    // 旋转相机
    private void RotateCamera(Vector2 input)
    {
        // 左右旋转（绕Y轴）
        float yaw = input.x * rotationSpeed * Time.deltaTime;
        cameraTransform.Rotate(0, yaw, 0, Space.World);

        // 上下旋转（绕X轴）
        float pitch = -input.y * rotationSpeed * Time.deltaTime;
        Vector3 currentRotation = cameraTransform.localEulerAngles;
        float newPitch = currentRotation.x + pitch;

        // 将角度转换为 -180 到 180 范围
        if (newPitch > 180) newPitch -= 360;

        // 限制上下旋转角度
        newPitch = Mathf.Clamp(newPitch, pitchMin, pitchMax);

        // 应用旋转
        cameraTransform.localEulerAngles = new Vector3(newPitch, currentRotation.y, currentRotation.z);
    }
}