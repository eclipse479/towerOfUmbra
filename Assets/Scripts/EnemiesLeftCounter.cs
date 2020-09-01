﻿using UnityEngine;
using UnityEngine.UI;

public class EnemiesLeftCounter : MonoBehaviour
{
    private Text counterText;
    private float enemyCounter;
    [Header("UIS")]
    [Tooltip("level complete UI")]
    public Canvas completeCanvas;
    [Tooltip("gameplay UI")]
    public Canvas gamePlayScreen;
    [Header("PLAYER BLOCKING OBJECT")]
    [Tooltip("a object to get disabled when there are no more enemies")]
    public GameObject playerBlockingWall;
    // Start is called before the first frame update
    void Start()
    {
        completeCanvas.enabled = false;
        counterText = gameObject.GetComponent<Text>();
    }

    public void subtract()
    {
        enemyCounter--;
        counterText.text = "Enemies left: " + enemyCounter;
        if(enemyCounter <= 0)
        {
            if(playerBlockingWall != null)
            {
                playerBlockingWall.SetActive(false);
                counterText.text = "all enemies have been slain";
            }
        }
    }

    public void add()
    {
        enemyCounter++;
        //counterText.text = "Enemies left: " + enemyCounter;
    }
    public void getNumberOfEnemies()
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag("enemy");
        enemyCounter = list.Length;
        counterText.text = "Enemies left: " + enemyCounter;
    }

    public float EnemiesLeft
    {
        get { return enemyCounter; }
    }
}
