using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkObject : MonoBehaviour
{
    [Header("ÉèÖÃ")]
    public float shrinkSpeed = 0.5f;  
    public float minSize = 0.1f;    

    void Update()
    {
        if (transform.localScale.x > minSize)
        {
            transform.localScale -= transform.localScale * shrinkSpeed * Time.deltaTime;
        }
    }
}
