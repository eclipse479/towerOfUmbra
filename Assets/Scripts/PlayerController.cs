using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float  movementSpeed = 5;
    public float jumpHeight = 36;
    public int health = 5;
    private bool grounded;
    private int layerMask = 1 << 8; // only hit layer 8
    // Start is called before the first frame update
    void Start()
    {
        grounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        //movement
        if(Input.GetKey(KeyCode.D)) //up
        {
            transform.Translate(new Vector3(0, 0, movementSpeed) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A)) //down
        {
            transform.Translate(new Vector3(0, 0, -movementSpeed) * Time.deltaTime);
        }
        if(!grounded)
        {
            transform.Translate(new Vector3(0, -2, 0) * Time.deltaTime);
        }
        //rotation
        if (Input.GetKey(KeyCode.Space)) //rotate left
        {
            transform.Translate(new Vector3(0, jumpHeight, 0) * Time.deltaTime);
        }

        RaycastHit groundRay;
        //forward raycast
        if (Physics.Raycast(transform.position, new Vector3(0,-1,0), out groundRay, 1, layerMask))
        {
            Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * groundRay.distance, Color.red);
        }
        else
        {
            Debug.DrawRay(transform.position, new Vector3(0, -1, 0), Color.blue);
        }

        if (health < 0)
        {
            Debug.Log("ded");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "bullet")
        {
            health--;
        }
    }
}
