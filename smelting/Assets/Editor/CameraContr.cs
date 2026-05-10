using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static Unity.VisualScripting.Member;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.GraphicsBuffer;
public class CameraContr : MonoBehaviour
{
    public bool Istouch = false;
    public List<GameObject> targetObjects = new List<GameObject>();
    public Camera cam;
    public GameObject prefab;
    public Vector3 spawnPosition;
    private PlayableDirector playDirector;
    public PlayableDirector Daomao;
    public GameObject highliaght;
    public GameObject water;
    public GameObject smallstone;
    public GameObject shihuishi;
    public GameObject kuangshifenmo;
    public GameObject Luzi;
    public GameObject Luzi1;
    public GameObject smoke;
    public GameObject fire;
    public GameObject tieshui;
    public GameObject mojushui;
    public GameObject Mutan;
    public GameObject player;
    public GameObject mobilecontroller;
    public GameObject Mojus;
    public GameObject R;
    public GameObject cutie;
    public GameObject[] tie;
    public bool tie_put1;
    public bool tie_put2;

    // Start is called before the first frame update


    void Start()
    {

        GameObject game = GameObject.FindGameObjectWithTag("Respawn");
        playDirector = game.GetComponent<PlayableDirector>();

        playDirector.stopped += OnTimelineStoped;
        Daomao.stopped += DaomoStoped;
        water.SetActive(false);
        fire.SetActive(false);
        mojushui.SetActive(false);
        shihuishi.SetActive(false);
        kuangshifenmo.SetActive(false);
        smoke.SetActive(false);
        tieshui.SetActive(false);
        cutie.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Touch touch = Input.GetTouch(0);
            HandleTouchClick(touch);



        }
    }


    void OnTimelineStoped(PlayableDirector director)
    {
        water.SetActive(false);
        playDirector.enabled = true;
        smallstone.GetComponent<BoxCollider>().enabled = true;
        tool.AddComponentToChildren<EmissionColorFader>(smallstone);
        Debug.Log("4444");

    }
    void HandleTouchClick(Touch touch)
    {

        Ray ray = cam.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 检测是否是目标物体
            if (targetObjects.Contains(hit.collider.gameObject))
            {
                Debug.Log("有效点击：" + hit.collider.name);
                OnHitTarget(hit.collider.gameObject);
            }
        }
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
    }

    void DaomoStoped(PlayableDirector director)
    {

        Vector3 finalRotation = cam.transform.rotation.eulerAngles;

        mojushui.SetActive(false);

        Mojus.transform.GetComponent<BoxCollider>().enabled = true;
        R.SetActive(true);
        tool.AddComponentToChildren<EmissionColorFader>(Mojus);
        cam.transform.rotation = Quaternion.Euler(finalRotation);

    }

    public void Active()
    {

        MobileControl mobile = FindObjectOfType<MobileControl>();
        mobile.enabled = false;

    }
    IEnumerator waitforcamera()
    {

        player.transform.position = new Vector3(0, 2.98000002f, -17.5f);
        yield return new WaitForSeconds(0.1f);
        mobilecontroller.transform.localPosition = Vector3.zero;
        mobilecontroller.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        cam.transform.localPosition = Vector3.zero;
        var instence = Instantiate(cutie);
        instence.name = "粗铁";
        instence.transform.localPosition = new Vector3(-0.375808716f, 2.21f, -24.200161f);
        instence.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        MobileControl mobile = FindObjectOfType<MobileControl>();
        mobile.enabled = true;




    }
    IEnumerator WaitThenActivate(GameObject game)
    {
        yield return new WaitForSeconds(3f);  // 等3秒
        game.SetActive(true);        // 激活新物体
        Debug.Log("新物体已激活");
        yield return new WaitForSeconds(6f);
        Active();
        player.transform.position = new Vector3(0, 2.98000002f, -22.5f);
        mobilecontroller.transform.position = new Vector3(0, 2.98000002f, -22.5f);
        mobilecontroller.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        cam.transform.position = new Vector3(0, 2.98000002f, -22.5f);
        cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        yield return new WaitForSeconds(0.5f);
        mojushui.SetActive(true);

        Daomao.enabled = true;
        Daomao.Play();
        Debug.Log("5555");

    }

    IEnumerator waitformove() {

       
        yield return new WaitForSeconds(3f);
        tie[0].AddComponent<EmissionColorFader>();
        tie[1].AddComponent<EmissionColorFader>();
        targetObjects.Add(tie[0]);
        targetObjects.Add(tie[1]);


    }

    void OnHitTarget(GameObject obj)
    {
        switch (obj.name)
        {
            case "iron":

                obj.SetActive(false);
                highliaght.SetActive(false);
                GameObject instance = Instantiate(prefab, new Vector3(6.17119217f, 0.639999986f, 7.73105288f), Quaternion.identity);
                instance.transform.localScale = new Vector3(50, 50, 50);
                instance.gameObject.name = "stone";
                instance.gameObject.AddComponent<EmissionColorFader>();
                targetObjects.Add(instance);
                break;

            case "stone":
                Destroy(obj);
                water.SetActive(true);
                playDirector.Play();
                break;


            case "IRONafterwh":
                Destroy(obj);
                Luzi.AddComponent<EmissionColorFader>();
                Luzi1.AddComponent<EmissionColorFader>();
                Luzi.gameObject.transform.parent.name = "高炉子";
                break;
            case "高炉子":

                //Luzi.GetComponent<EmissionColorFader>().enabled =false;
                //Luzi1.GetComponent<EmissionColorFader>().enabled = false;

                Luzi1.transform.DOMoveZ(-10f, 2f).OnComplete(() =>
                {
                    Destroy(Luzi1);
                    shihuishi.SetActive(true);
                    shihuishi.transform.DOMoveY(2f, 1f).OnComplete(() =>
                    {

                        kuangshifenmo.SetActive(true);
                        kuangshifenmo.transform.DOMoveY(3f, 1f).OnComplete(() =>
                        {

                            Mutan.GetComponent<FireController>().enabled = true;
                            fire.SetActive(true);
                            smoke.SetActive(true);

                            tool.AddComponentToChildren<ShrinkObject>(kuangshifenmo.transform.GetChild(0).gameObject);
                            tool.AddComponentToChildren<ShrinkObject>(kuangshifenmo.transform.GetChild(1).gameObject);
                            tool.AddComponentToChildren<ShrinkObject>(shihuishi.transform.GetChild(0).gameObject);
                            tool.AddComponentToChildren<ShrinkObject>(shihuishi.transform.GetChild(1).gameObject);


                            StartCoroutine(WaitThenActivate(tieshui));

                        });

                    });
                });



                //tieshui.SetActive(true);
                break;

            case "MOJU":
                targetObjects.Add(cutie);
                cutie.SetActive(true);
                R.SetActive(false);
                tool.RemoveComponentFromChildren<EmissionColorFader>(Mojus);
                cutie.AddComponent<EmissionColorFader>();
                Mojus.GetComponent<BoxCollider>().enabled = false;
                break;

            case "cutie":

                StartCoroutine(waitforcamera());

                break;

            case "粗铁":
                obj.transform.DOScale(new Vector3(0.239999995f, 0.239999995f, 0.720000029f), 3f);
                StartCoroutine(waitformove());
                break;

            case "tie":
                var instence = GameObject.Find("粗铁");
                tie[0].GetComponent<EmissionColorFader>().enabled =false;
                obj.transform.DOMove(instence.transform.position + Vector3.up * 0.24f, 1f);
               
             targetObjects.Remove(tie[0]);
                if (!tie_put1)
                {
                    tie_put1 = true;
                }
                break;
            case "tie1":
                var instence1 = GameObject.Find("粗铁");
                targetObjects.Remove(tie[1]);
                tie[1].GetComponent<EmissionColorFader>().enabled = false;
                obj.transform.DOMove(instence1.transform.position + Vector3.up * -0.24f, 1f);
                if (!tie_put2)
                {
                    tie_put2 = true;
                }
                break;
               
            default:

                break;


                
        }

     
    }
}
    
