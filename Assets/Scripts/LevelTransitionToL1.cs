using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionToL1 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            Debug.Log("player has entered");
            SceneManager.LoadScene(1);

            SoundManager.instance.stopSound("tutorialAmbience");
            SoundManager.instance.stopSound("tutorialMusic");
            SoundManager.instance.playSound("caveAmbience");
            SoundManager.instance.playSound("caveMusic");

        }
        
    }
 
}
