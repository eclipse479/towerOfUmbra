using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionToL3 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            Debug.Log("player has entered");
            SceneManager.LoadScene(3);
            SoundManager.instance.stopSound("TorchesBurning");
        }
        
    }
 
}
