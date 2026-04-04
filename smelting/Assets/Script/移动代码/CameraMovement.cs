using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public VirtualJoystick movementJoystick; // 左摇杆引用
    public float moveSpeed = 5f;             // 移动速度

    private Transform cameraTransform; // 相机的Transform组件

    void Start()
    {
        // 获取相机的Transform组件
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 如果左摇杆有输入，则移动相机
        if (movementJoystick != null && movementJoystick.Input != Vector2.zero)
        {
            MoveCamera(movementJoystick.Input);
        }
    }

    // 移动相机
    private void MoveCamera(Vector2 input)
    {
        // 计算移动方向
        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;

        // 将移动方向转换为世界坐标系
        moveDirection = cameraTransform.TransformDirection(moveDirection);

        // 移动相机
        cameraTransform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}