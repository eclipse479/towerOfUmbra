﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeGML2 : MonoBehaviour
{
    
    public KeyCode restartKey = KeyCode.R;


    public KeyCode QuitKey = KeyCode.Q;
    // Start is called before the first frame update
    void Start()
    {

    }


    void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            SceneManager.LoadScene(2);
        }

        if (Input.GetKeyDown(QuitKey))
        {
            Application.Quit();
            Debug.Log("Bye");
        }
    }
}
