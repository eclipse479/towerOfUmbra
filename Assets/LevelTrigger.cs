using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    LevelLoader levelLoader;

    private void Awake()
    {
        levelLoader = GameObject.Find("LevelChanger").GetComponent<LevelLoader>();
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
        GameObject other = collision.gameObject;

        if (other.layer == 8)
        {
            levelLoader.LoadLevelNext();
        }
    }
}
