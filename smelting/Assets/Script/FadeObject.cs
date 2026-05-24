using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{

 

    void OnEnable()
    {
        GetComponent<Renderer>().material.DOFade(1f, 2f);
    }

   
    // Update is called once per frame
    void Update()
    {
        
    }
}
