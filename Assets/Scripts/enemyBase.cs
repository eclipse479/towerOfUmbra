using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemyBase : MonoBehaviour
{
    private Slider healthSlider;
    public int health;
    /// <summary>
    /// script to hold things that all enemies will have
    /// </summary>
    void Start()
    {
        Transform child = transform.GetChild(0);
        healthSlider = child.GetChild(0).GetComponent<Slider>();
        healthSlider.maxValue = health;
        healthSlider.value = health;
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
            healthSlider.value = health;
        }
    }
}
