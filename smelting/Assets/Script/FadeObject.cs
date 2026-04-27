using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.DOFade(0f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
