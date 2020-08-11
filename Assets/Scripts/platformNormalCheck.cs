using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformNormalCheck : MonoBehaviour
{
    Renderer rendd;
    // Start is called before the first frame update
    void Start()
    {
        rendd = gameObject.GetComponent<Renderer>();
        rendd.material.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionExit(Collision collision)
    {
        rendd.material.color = Color.white;
    }
}
