using System;
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
    private bool extending;
    private bool active;
    //has the hook hit an enemy
    private bool isEnemyGrabbed;
    private bool wallGrabbed;
    //the player
    public GameObject player;
    private Rigidbody playerRB;
    //how strong the grapple to a wall is
    public float forcePower;
    public float enemyPullForce;
    //rotation point
    private GameObject parent;
    //tip of the grappling hook(holds the enemy)
    private GameObject tip;
    //enemy hit by grappling hook
    private GameObject grabbedEnemy;
    //stop grapling enemy distance
    public float stopEnemyGrappleDistance;
    //vector for pulling direction player -> wall
    private Vector3 forceDirection;
    // Start is called before the first frame update
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
        playerRB = player.GetComponent<Rigidbody>();
        tip = gameObject.transform.GetChild(0).gameObject;
        extending = false;
        active = false;
        isEnemyGrabbed = false;
        Physics.IgnoreLayerCollision(0, 8);
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
                    wallGrabbed = false;
                    isEnemyGrabbed = false;
                    Vector3 destination = hit.point; // position clicked
                    parent.gameObject.transform.LookAt(new Vector3(destination.x, destination.y, 0.25f));
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
                //gets distance from grabbed enemy to player
                Vector3 distance = (transform.position - grabbedEnemy.transform.position);
                float distanceToPlayer = distance.magnitude;
                if (distanceToPlayer < stopEnemyGrappleDistance)
                { //remove grabbed enemy
                    grabbedEnemy = null;
                    //enemy is not grabbed anymore
                    isEnemyGrabbed = false;
                }
                else if (distanceToPlayer > stopEnemyGrappleDistance)
                {
                    //pulls object to player
                    pullObject(grabbedEnemy);
                }
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
        //collisions with various objects with tags
        if(collision.gameObject.tag == "clickWall")
        {
            //ignore the click wall
        }
        else if(collision.gameObject.tag == "enemy" && !isEnemyGrabbed && !wallGrabbed)
        {
            //grapple the enemy
            grabbedEnemy = collision.gameObject;
            isEnemyGrabbed = true;
            extending = false;
            Debug.Log(collision.gameObject);
        }
        else if (collision.gameObject.tag == "grappleTarget" && !wallGrabbed && !isEnemyGrabbed)
        {
            //grapple the wall
            playerRB.velocity = Vector3.zero;
            grabbedEnemy = collision.gameObject;
            isEnemyGrabbed = false;
            wallGrabbed = true;
            extending = false;
            forceDirection = (tip.gameObject.transform.position - player.gameObject.transform.position);
            forceDirection.Normalize();
            Debug.Log("X: " + forceDirection.x + " Y: " + forceDirection.y + " Z: " + forceDirection.z);
        }
        else 
        {
            //hits anything else
            extending = false;
            Debug.Log(collision.gameObject);
        }
    }
    private void pullObject(GameObject thingToPull)
    {
        Rigidbody enemyBody;
        enemyBody = thingToPull.GetComponent<Rigidbody>();
        Vector3 enemyDirection = (player.transform.position - thingToPull.transform.position);
        enemyDirection.Normalize();
        enemyBody.AddForce(enemyDirection * enemyPullForce);
    }
    private void playerPullToWall()
    {
        playerRB.AddForce(forceDirection * forcePower);
    }


}
