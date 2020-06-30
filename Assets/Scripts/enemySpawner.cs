using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemySpawner : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public Text timerText;
    public float maxSpawnTimer;
    private float spawnTimer;
    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = maxSpawnTimer;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;
        string timerString = spawnTimer.ToString("F2");
        timerText.text = "Time until new enemy spawn: " + timerString;
        if(spawnTimer < 0)
        {
            spawnTimer = maxSpawnTimer;
            Instantiate(enemyToSpawn, gameObject.transform.position, gameObject.transform.rotation);
        }
    }
}
