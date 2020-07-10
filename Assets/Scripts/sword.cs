using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sword : MonoBehaviour
{
    private playerController1 control;
    // Start is called before the first frame update
    void Start()
    {
        GameObject swordBase = gameObject.transform.parent.gameObject;
        GameObject parent = swordBase.gameObject.transform.parent.gameObject;
        control = parent.GetComponent<playerController1>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "enemy")
        {
            control.swordCollision();
        }
    }
}
