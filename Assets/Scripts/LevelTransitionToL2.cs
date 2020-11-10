using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionToL2 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            Debug.Log("player has entered");
            SoundManager.instance.stopSound("TorchesBurning");
            SceneManager.LoadScene(2);
        }
        
    }
 
}
