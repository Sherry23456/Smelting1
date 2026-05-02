using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scencejump : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Entermain()
    {
        SceneManager.LoadScene(1);
    }
    public void Entersmelting()
    {
        SceneManager.LoadScene(2);
    }
    public void Exitsmelting()
    {
        SceneManager.LoadScene(1);
    }
}
