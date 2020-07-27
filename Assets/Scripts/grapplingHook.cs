﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class grapplingHook : MonoBehaviour
{
    //total length of the hook
    public float maxLength;
    //how fast it extends
    public float extendRate;
    //is the hook extended

    private bool active;
    //has the hook hit an enemy
    //the player
    public GameObject player;
    //rotation point
    private GameObject parent;
    //tip of the grappling hook(holds the enemy)
    private GameObject tip;

    //are changed in another script
    [HideInInspector]
    public bool extending;
    [HideInInspector]
    public Rigidbody playerRB;
    [HideInInspector]
    public bool isEnemyGrabbed;
    [HideInInspector]
    public bool wallGrabbed;
    //enemy hit by grappling hook
    [HideInInspector]
    public GameObject grabbedEnemy;
    //vector for pulling direction player -> wall
    [HideInInspector]
    public Vector3 forceDirection;
    private float hold;

    private float playerZ;

    // Start is called before the first frame update
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
        playerRB = player.GetComponent<Rigidbody>();
        tip = gameObject.transform.GetChild(0).gameObject;
        extending = false;
        active = false;
        isEnemyGrabbed = false;
        parent.transform.position = player.transform.position;
        playerZ = player.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        //moves grapple to player position
        parent.transform.position = player.transform.position;


        //extending and returning grapple
        if (parent.transform.localScale.z > maxLength * 10)
        {
            extending = false;
        }
        else if (parent.transform.localScale.z < 0.1f)
        {
            isEnemyGrabbed = false;
            wallGrabbed = false; 
            extending = true;
            active = false;
        }

        //is a grapple currently being shot out
        if (!active)
        {
            //gets a new spot to shoot a grapple towards
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;


                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    active = true; // grapple exists
                    extending = true; // grapple is extending
                    Vector3 destination = hit.point; // position clicked
                    //make grapple face position clicked to it can extend properly
                    parent.gameObject.transform.LookAt(new Vector3(destination.x, destination.y, playerZ));
                }
            }
        }
        //grapple exists
        if (active)
        {
            if (extending) // growing
            {
                parent.transform.localScale += new Vector3(0, 0, extendRate) * 10 * Time.deltaTime;
            }
            else if (!extending)//shrinking
            {
                parent.transform.localScale += new Vector3(0, 0, -extendRate) * 10 * Time.deltaTime;
            }
            //stops negative scale values
            if (parent.transform.localScale.z < 0)
            {
                parent.transform.localScale = new Vector3(1, 1, 0.01f);
                active = false;
            }
        }

        //grabbing enemy/wall
        if (!extending)//if shrinking
        {
            if (isEnemyGrabbed)
            {
                //pulls object to player
                pullEnemy(grabbedEnemy);
            }
            else if (wallGrabbed)
            {
                //pulls player to wall
                playerPullToWall();
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject);
        //collisions with various objects with tags
        if(collision.gameObject.tag == "enemy" && !isEnemyGrabbed && !wallGrabbed && extending)
        {
            //grapple the enemy
            grabbedEnemy = collision.gameObject;
            isEnemyGrabbed = true;
            extending = false;
        }
        else if (collision.gameObject.tag == "grappleTarget" && !wallGrabbed && !isEnemyGrabbed && extending)
        {
            //grapple the wall
            playerRB.velocity = Vector3.zero;
            isEnemyGrabbed = false;
            wallGrabbed = true;
            extending = false;
            forceDirection = (tip.gameObject.transform.position - player.gameObject.transform.position);
            hold = forceDirection.magnitude;
            forceDirection.Normalize();
        }
        else 
        {
            //hits anything else
            extending = false;
        }
    }

    public void pullEnemy(GameObject thingToPull)
    {
        isEnemyGrabbed = false;
        Rigidbody enemyBody;
        enemyBody = thingToPull.GetComponent<Rigidbody>();
        Vector3 enemyDirection = (player.transform.position - thingToPull.transform.position);
        float distance = enemyDirection.magnitude;
        Debug.Log("force: " + CalculateJumpForce(distance, 9.8f));
        Debug.Log("direction X: " + enemyDirection.x + " direction Y: " + enemyDirection.y + " direction Z: " + enemyDirection.z);
        enemyDirection.Normalize();
        enemyBody.AddForce(enemyDirection * CalculateJumpForce(distance,9.8f),ForceMode.Impulse);
    }
    public void playerPullToWall()
    {
        wallGrabbed = false;
        playerRB.AddForce(forceDirection * CalculateJumpForce(hold, 9.8f), ForceMode.Impulse);
    }

    private float CalculateJumpForce(float jumpHeight, float gravity)
    {
        //jumpheight -> distance to target to jump to
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
}
