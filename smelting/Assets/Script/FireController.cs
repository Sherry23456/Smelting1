using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public Material burningCharcoal;

    private void OnEnable()
    {
        burningCharcoal.SetFloat("_BurnAmount", 0.78f);
    }
    private void OnDisable()
    {
        burningCharcoal.SetFloat("_BurnAmount", 0.78f);
    }
    public void SetFireIntensity(float intensity)
    {
        burningCharcoal.SetFloat("_BurnAmount", Mathf.Clamp01(intensity));
    }

    void Update()
    {
        float time = Time.time;
        float intensity = Mathf.PingPong(time*0.5f, 1f) * 0.6f;  // 0-3 Ñ­»·

        SetFireIntensity(intensity);
       
    }
}
