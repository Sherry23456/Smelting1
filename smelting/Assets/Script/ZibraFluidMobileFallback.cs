using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 安卓端 Zibra Liquids 替代显示管理器。
/// 免费版 Zibra 没有安卓原生库（ZibraFluidNative_Android.so），实机上流体不渲染，
/// 且 ZibraLiquid.OnEnable 会抛 DllNotFoundException（插件内已加平台守卫）。
/// 本组件放在常驻物体上，维护若干组 (Zibra容器, 替代物) 配对：
/// - 移动端：Awake 时禁用各容器上的所有 Zibra 组件（避免任何桥接调用），
///   每帧把替代物的显隐同步为容器的激活状态（容器仍由 CameraContr 正常开关）；
/// - PC 端：只确保替代物隐藏，Zibra 流体保持原样。
/// 用每帧同步代替 OnEnable/OnDisable 联动，避免激活时序问题。
/// </summary>
[DefaultExecutionOrder(-32000)]
public class ZibraFluidMobileFallback : MonoBehaviour
{
    [System.Serializable]
    public class FluidPair
    {
        [Tooltip("Zibra 流体容器（如 水锤/water、高炉/lava/lava）")]
        public GameObject zibraContainer;
        [Tooltip("移动端替代显示物（容器的子物体，随容器显隐）")]
        public GameObject replacement;
    }

    [Tooltip("Zibra 容器与替代物的配对")]
    public List<FluidPair> pairs = new List<FluidPair>();

    [Tooltip("编辑器中模拟移动端行为用于验证，仅影响编辑器")]
    public bool simulateMobileInEditor = false;

    private bool _mobile;

    /// <summary>当前是否处于“无 Zibra 原生库”的移动端环境</summary>
    public bool IsMobileWithoutZibra
    {
        get
        {
            if (Application.isEditor)
                return simulateMobileInEditor;
            return Application.platform == RuntimePlatform.Android ||
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }
    }

    private void Awake()
    {
        _mobile = IsMobileWithoutZibra;
        if (!_mobile)
        {
            // PC：确保替代物不显示
            foreach (var p in pairs)
            {
                if (p != null && p.replacement != null)
                    p.replacement.SetActive(false);
            }
            return;
        }

        Debug.Log("[ZibraFluidMobileFallback] 移动端：禁用 Zibra 组件，启用替代显示");
        foreach (var p in pairs)
        {
            if (p == null || p.zibraContainer == null)
                continue;
            DisableZibraComponents(p.zibraContainer.transform);
        }
    }

    private void Update()
    {
        if (!_mobile)
            return;

        foreach (var p in pairs)
        {
            if (p == null || p.replacement == null || p.zibraContainer == null)
                continue;
            bool shouldShow = p.zibraContainer.activeInHierarchy;
            if (p.replacement.activeSelf != shouldShow)
                p.replacement.SetActive(shouldShow);
        }
    }

    private static void DisableZibraComponents(Transform root)
    {
        foreach (var comp in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (comp == null)
                continue;
            var type = comp.GetType();
            if (type.Namespace != null && type.Namespace.StartsWith("com.zibra.liquid"))
                comp.enabled = false;
        }
    }
}
