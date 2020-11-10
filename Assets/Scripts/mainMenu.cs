using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenu : MonoBehaviour
{
    public playerController1 player;
    // Start is called before the first frame update
    void Start()
    {
        player.setMainmenu(true);
    }

    public void startButtonClicked()
    {
        player.setMainmenu(false);
        gameObject.SetActive(false);
        SoundManager.instance.playSound("Click");
    }

    public void quitButtonPressed()
    {
        Application.Quit();
        SoundManager.instance.playSound("Click");
        Debug.Log("quit");
    }
     
}
