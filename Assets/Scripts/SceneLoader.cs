using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void Quitgame()
    {
        Application.Quit();
        Debug.Log("I quit!");
    }

    public void restartFromTutorial()
    {
        SoundManager.instance.stopSound("EndingMusic");
        SoundManager.instance.stopSound("EndingAmbieance");
        SoundManager.instance.playSound("TutorialMusic");
        SoundManager.instance.playSound("TutorialAmbience");
        playerStats.health = playerStats.maxHealth;
        SceneManager.LoadScene(0);
    }
}

