using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionToEnd : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            Debug.Log("player has entered");
            SceneManager.LoadScene(4);
            SoundManager.instance.stopSound("caveMusic");
            SoundManager.instance.stopSound("caveAmbience");
            SoundManager.instance.playSound("TorchesBurning");
            SoundManager.instance.playSound("EndingMusic");
        }
        
    }
 
}
