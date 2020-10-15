using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grappleClaw : MonoBehaviour
{
    private List<MeshRenderer> rends;
    public float maxLength;
    public float extendRate;

    public Transform mainCam, player, grappleTip;
    public LayerMask grappleable;

    // Start is called before the first frame update
    void Awake()
    {
        //renderers of the grappling hook;
        rends = new List<MeshRenderer>();
        for (int i = 1; i <= 3; i++)
        {
            rends.Add(gameObject.transform.GetChild(i).GetComponent<MeshRenderer>());
        }
        for (int i = 0; i < 3; i++)
        {
            rends.Add(gameObject.transform.GetChild(0).gameObject.transform.GetChild(i).GetComponent<MeshRenderer>());
        }
        disappear();
    }

    public void disappear()
    {
        Debug.Log("dissapear");
        foreach (MeshRenderer rend in rends)
        {
            rend.enabled = false;
        }
    }

    public void reappear()
    {
        Debug.Log("reappear");
        foreach (MeshRenderer rend in rends)
        {
            rend.enabled = true;
        }
    }
}
