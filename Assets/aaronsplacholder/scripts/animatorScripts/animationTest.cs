using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationTest : MonoBehaviour
{
    public GameObject cobe;
    public BoxCollider handBox;
    public void PrintEvent(string s)
    {
        Debug.Log("PrintEvent: " + s + " called at: " + Time.time);
    }

    public void createCobe()
    {
        Instantiate(cobe, transform.position, transform.rotation);
    }

    public void handOn()
    {
        Debug.Log("HandisOn");
        handBox.enabled = true;
    }

    public void handOff()
    {
        Debug.Log("HandisOff");
        handBox.enabled = false;
    }
}
