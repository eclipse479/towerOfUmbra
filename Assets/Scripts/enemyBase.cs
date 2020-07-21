using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyBase : MonoBehaviour
{

    public int health;
    /// <summary>
    /// script to hold things that all enemies will have
    /// </summary>
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "swordBlade")
        {
            health--;
        }
    }
}
