using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guide : MonoBehaviour
{
    TextTypewriter _typewriter;
   
  
    void Awake()
    {
        _typewriter = FindObjectOfType<TextTypewriter>();
    }

    
    void Update()
    {
       
    }

    public void ShowNewText(string text)
    {
        _typewriter.ShowText(text, 0.05f);
    }
}
