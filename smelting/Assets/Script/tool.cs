using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class tool 
{
    public static void AddComponentToChildren<T>(GameObject parent) where T : Component
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            Debug.Log(child);
            if (child.gameObject != parent && child.gameObject.activeSelf)
            {
                if (!child.gameObject.TryGetComponent<T>(out _))
                {
                    child.gameObject.AddComponent<T>();
                }
            }
        }
    }
}
