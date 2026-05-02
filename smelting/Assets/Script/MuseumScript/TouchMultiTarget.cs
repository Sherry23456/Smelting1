using TMPro;
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
    [Header("预制体")]
    public GameObject[] yuzhiti;
    [Header("预制体父物体")]
    public GameObject fatherobj;
    public TMP_Text text1;
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
    /// <summary>
    /// 预制体
    /// </summary>
    void YuzhiiX(int a)
    {
        GameObject newyuzhiti = Instantiate(yuzhiti[a]);
        newyuzhiti.transform.SetParent(fatherobj.transform);
        newyuzhiti.transform.position = new Vector3(545.112976f, 146.203995f, 443);
        newyuzhiti.transform.rotation = Quaternion.identity;
        if (a == 2)
        {
            newyuzhiti.transform.localScale = new Vector3(80, 80, 80);
        }
        else
        {
            newyuzhiti.transform.localScale = new Vector3(50, 50, 50);
        }

    }
    /// <summary>
    /// 删除预制体
    /// </summary>
    public void Dedtroyobj()
    {
        foreach (Transform item in fatherobj.transform)
        {
            Destroy(item.gameObject);
        }
    }
    void OnHitTarget(GameObject obj)
    {
        switch (obj.name)
        {
            case "tieli": //铁犁旋转
                Dedtroyobj();
                YuzhiiX(0);
                text1.text = "";
                text1.text = "<b>铁耙</b>始于北魏，距今已有 1500 余年历史，是古代冶铁技术应用于农耕的经典器具。其主体铁齿经冶炼锻造而成，坚硬耐用，耕作深度不超过 15 厘米。《齐民要术》称其 “铁齿耙”，《王祯农书》载有方耙、人字耙等形制。铁耙结构简约实用，既体现古人冶金工艺水平，也承载着传统农耕智慧。";
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
            case "qingtongqi": //青铜器旋转
                Dedtroyobj();
                YuzhiiX(1);
                text1.text = "";
                text1.text = "<b>颂壶</b>为西周晚期青铜礼器，距今近三千年，是青铜范铸工艺与礼乐文明的代表。器形庄重，饰龙纹、环带纹，颈部兽首衔环，内壁铸有 151 字铭文，记载册命礼制。此复刻品遵循古法青铜冶炼铸造，再现西周高超冶金技艺与礼器神韵。";
                uiimage[0].SetActive(true);
                cam.GetComponent<TouchCameraController2>().enabled = false;
                joystick.SetActive(false);
                break;
            case "bingqi"://兵器旋转
                Dedtroyobj();
                YuzhiiX(2);
                text1.text = "";
                text1.text = "<b>剑</b>，被誉为 “百兵之君”，是古代冶金技术的巅峰代表。早期为青铜铸造，吴越铸剑工艺冠绝天下，如越王勾践剑采用复合金属与防锈工艺。汉代后以百炼钢、淬火技术锻打钢铁剑，刃利坚韧。它既是实战兵器，也体现了古代冶炼技艺与礼乐文化的高度融合。";
                uiimage[0].SetActive(true);
                cam.GetComponent<TouchCameraController2>().enabled = false;
                joystick.SetActive(false);
                break;
            case "冶炼":



                break;
            case "door_box":
                joystick.SetActive(false);
                uiimage[1].SetActive(true);
                break;
            default:
                break;
        }
    }
   
}