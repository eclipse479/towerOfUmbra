using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class basicEnemy : MonoBehaviour
{
    //objects to movetowards
    private GameObject player;
    private GameObject[] patrol;
    //text displays the movement type
    private Renderer rend;
    //navMeshAgent
    private NavMeshAgent agent;
    //how the agent moves
    private int moveType;
    private int holdMoveType;
    private int destinationPoint;
    private bool moveTypeHeld;
    //projectile
    public GameObject bullet;
    //shooting timer
    private float timer;
    public float maxTimer;

    public float agroDistance;
    private bool canFire;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("player");
        agent = gameObject.GetComponent<NavMeshAgent>();
        rend = gameObject.GetComponent<Renderer>();
        patrol = GameObject.FindGameObjectsWithTag("patrolPoint");
        agent.autoBraking = false;

        timer = maxTimer;
        moveType = Random.Range(0,2);
        moveTypeHeld = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && moveType != 4)
        {
            if (moveType < 2)
            {
                moveType++;
            }
            else
            {
                moveType = 0;
            }
        }
        
        //move agent towards player
        if (moveType == 0)
        {
            agent.destination = player.transform.position;
            rend.material.color = Color.red;
        }
        //move agent to where the mouse was clicked
        else if (moveType == 1)
        {
            rend.material.color = Color.blue;
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    agent.destination = hit.point;
                }
            }
        }
        //patrol through various points
        else if (moveType == 2)
        {
            rend.material.color = Color.green;
            // if the path has been found and the distance to the target in < 0.5 units find next path
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            { goToNextPoint(); }
        }
        //stops in its position
        else if (moveType == 4)
        {
            rend.material.color = Color.black;
            agent.destination = agent.transform.position;
        }
        Vector3 dist = agent.transform.position - player.transform.position;
        float   distance = dist.magnitude;
        //if the enemy is less than 5 units away from player wait 1 sec then fire a bullet towards them then continue
        if(distance <= agroDistance)
        {
            transform.LookAt(player.transform.position);
            if (!moveTypeHeld)
            { 
                holdMoveType = moveType;
                moveTypeHeld = true;
                canFire = true;
                moveType = 4;
                timer = maxTimer;
            }
            
        }
        else if(distance > agroDistance && moveTypeHeld)
        {
            moveTypeHeld = false;
            moveType = holdMoveType;
        }

        //shooting timer
        if (timer > 0)
        {
            timer -= Time.deltaTime; 
        }
        //fire a bullet if timer allows it and can fire
        if(timer < 0 && canFire)
        {
            canFire = false;
            moveTypeHeld = false;
            //fire bullet then reset timer
            Instantiate(bullet, transform.position, transform.rotation);
            timer = maxTimer;
            moveType = holdMoveType;
        }
    }

    private void goToNextPoint()
    {
        //stops error in case there are no points for patroling
        if (patrol.Length == 0)
            return;
        //changes destination to the next point in the list, looping to the start at the end
        agent.destination = patrol[destinationPoint].transform.position;
        destinationPoint = (destinationPoint + 1) % patrol.Length;

    }
}
