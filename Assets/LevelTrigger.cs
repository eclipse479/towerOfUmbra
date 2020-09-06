using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    LevelLoader levelLoader;
    EnemiesLeftCounter counter;

    private void Awake()
    {
        levelLoader = GameObject.Find("LevelChanger").GetComponent<LevelLoader>();
        counter = GameObject.Find("enemiesLeft").GetComponent<EnemiesLeftCounter>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (counter.EnemiesLeft == 0)
        {
            GameObject other = collision.gameObject;

            if (other.layer == 8)
            {
                levelLoader.LoadLevelNext();
            }
        }
    }
}
