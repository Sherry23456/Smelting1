using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    public Vector3 pivotPoint;        // 支点位置（锤柄固定点）
    public float rotateSpeed = 5f;    // 摆动速度
    public float minAngle = -45f;     // 最低角度
    public float maxAngle = 45f;      // 最高角度
    public Transform pivot;           // 支点transform（可为空，为空则用pivotPoint）

    private float currentAngle;

    private void Awake()
    {
        pivotPoint = pivot.position;
    }
    void Update()
    {
        float targetAngle = Mathf.Abs(Mathf.Sin(Time.time * rotateSpeed) * maxAngle);
        //float tempAngle = Mathf.Sin(Time.time * rotateSpeed) * maxAngle;
        //float targetAngle = tempAngle < 0 ? -tempAngle : tempAngle;


        float angleDelta = targetAngle - currentAngle;
        currentAngle = targetAngle;

    
        transform.RotateAround(pivotPoint, Vector3.forward, angleDelta);
    }
}