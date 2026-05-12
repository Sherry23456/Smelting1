using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EmissionColorFader : MonoBehaviour
{
    // 颜色区间
    public Color startColor = Color.black; // 起始颜色（通常设为黑色或暗色）
    public Color endColor = Color.white;   // 结束颜色（通常设为亮色）

    // 变化速度（单位：秒，值越小速度越快）
    public float duration = 2f;

    // 是否循环往复（如果需要一次性变化，可以关闭）
    public bool loop = true;

    // 计时器
    private float elapsed = 0f;
    private Renderer rend;
    private Material materialInstance;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // 实例化材质（防止影响共享材质）
        materialInstance = rend.material;

        // 开启自发光关键字（Emission）
        materialInstance.EnableKeyword("_EMISSION");
        materialInstance.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // 初始化颜色
        materialInstance.SetColor("_EmissionColor", startColor);
    }

    void Update()
    {
      
        elapsed += Time.deltaTime;

        
        float t = Mathf.PingPong(elapsed / duration, 1f);

       
        Color curColor = Color.Lerp(startColor, endColor, t);

        
        materialInstance.SetColor("_EmissionColor", curColor);
    }
    
    public void OnDisable()
    {
        materialInstance.SetColor("_EmissionColor", startColor);
    }
}