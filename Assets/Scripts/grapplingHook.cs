using UnityEngine;
using UnityEngine.Animations;

public class grapplingHook : MonoBehaviour
{
    //total length of the hook
    [Header("GRAPPLE STATS")]
    [Tooltip("how far the grapple can move")]
    public float maxLength;
    //how fast it extends
    [Tooltip("how fast the grapple moves")]
    public float extendRate;
    //is the hook extended

    private bool active;
    //has the hook hit an enemy
    //the player
    [Header("PLAYER STATS")]
    [Tooltip("the player")]
    public GameObject player;
    //rotation point
    //camera stats
    [Tooltip("the camera that follows the player")]
    public Camera playerCamera;
    [Tooltip("how far the grapple is from the player")]
    public float grappleDistFromPlayer;
    
    private GameObject parent;

    public float baseHeightIncrease;

    private Rigidbody playerRB;
    private Rigidbody rb;
    private bool extending;
    private bool isEnemyGrabbed;
    private bool wallGrabbed;
    //enemy hit by grappling hook
    [HideInInspector]
    public GameObject grabbedEnemy;
    //vector for pulling direction player -> wall
    [HideInInspector]
    public Vector3 forceDirection;
    private float distanceToWall;

    private Collider collide;
    [Header("FORCE MULTIPLIERS")]
    [Tooltip("force multiplier when a wall is hit with the grapple")]
    public float grapplePullToWallForce;
    [Tooltip("force multiplier when an enemy is hit by the grapple")]
    public float grapplePullEnemyForce;

    private LineRenderer lRend;
    // Start is called before the first frame update
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
        lRend = transform.GetComponent<LineRenderer>();
        collide = gameObject.GetComponent<Collider>();
        rb = transform.GetComponent<Rigidbody>();
        playerRB = player.GetComponent<Rigidbody>();
        collide.enabled = false;
        isEnemyGrabbed = false;
        extending = false;
        active = false;
        parent.transform.position = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //moves grapple to player position
        parent.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + baseHeightIncrease, player.transform.position.z) + (playerCamera.transform.forward * grappleDistFromPlayer);
        //sets the line renderer to draw between the hook and parent
        lRend.SetPosition(0, parent.transform.position);
        lRend.SetPosition(1, transform.position);

        //the distance from the parent to the hook
        Vector3 distanceParentToPointVec = parent.transform.position - transform.position;
        float distanceFormParentToPointFloat = distanceParentToPointVec.magnitude;

        //if the grapple is active
        if (active)
        {
            if (extending) // growing
            {
                //rb.MovePosition(transform.position + (transform.right * extendRate * Time.deltaTime));
                transform.position += parent.transform.forward * extendRate * Time.deltaTime;
            }
            else if (!extending)//shrinking
            {

                //rb.MovePosition(transform.position - (transform.right * extendRate * Time.deltaTime));
                transform.position -= parent.transform.forward * extendRate * Time.deltaTime;
            }
        }
        else if (!active)
        {
            //gets a new spot to shoot a grapple towards
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;


                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    Vector3 destination = hit.point; // position clicked
                    //make grapple face position clicked to it can extend properly
                    parent.gameObject.transform.LookAt(new Vector3(destination.x, destination.y, transform.position.z));
                    collide.enabled = true;
                    active = true; // grapple exists
                    extending = true; // grapple is extending
                    rb.isKinematic = false;
                }
            }
        }

        if (distanceFormParentToPointFloat > maxLength)
        {
            extending = false;
            collide.enabled = false;
        }
        
        if (!extending)
        {
            rb.isKinematic = true;
            Vector3 toTarget = (parent.transform.position - transform.position).normalized;
            //if(Vector2.Dot(new Vector2(parent.transform.position.x, parent.transform.position.y), new Vector2(transform.position.x, transform.position.y)) > 0)
            if (Vector3.Dot(toTarget, transform.right) > 0) // checks if the parents position is in front of the hook
            {
                Debug.Log("parent is in front of this game object.");
                isEnemyGrabbed = false;
                wallGrabbed = false; 
                active = false;
                transform.position = parent.transform.position;
            }
               
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //collisions with various objects with tags
        if(collision.gameObject.tag == "enemy" && !isEnemyGrabbed && !wallGrabbed && extending)
        {
            //grapple the enemy
            grabbedEnemy = collision.gameObject;
            isEnemyGrabbed = true;
            extending = false;
            pullEnemy(grabbedEnemy);
        }
        else if (collision.gameObject.tag == "grappleTarget" && !wallGrabbed && !isEnemyGrabbed && extending)
        {
            //grapple the wall
            playerRB.velocity = Vector3.zero;
            isEnemyGrabbed = false;
            wallGrabbed = true;
            extending = false;
            forceDirection = (new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, player.gameObject.transform.position.z) - player.gameObject.transform.position);
            distanceToWall = forceDirection.magnitude;//length
            forceDirection.Normalize();
            playerPullToWall();
        }
        else 
        {
            //hits anything else
            extending = false;
        }
    }

    public void pullEnemy(GameObject thingToPull)
    {
        collide.enabled = false;
        isEnemyGrabbed = false;
        Rigidbody enemyBody;
        enemyBody = thingToPull.GetComponent<Rigidbody>();
        Vector3 enemyDirection = (player.transform.position - thingToPull.transform.position);
        float distance = enemyDirection.magnitude;
        enemyDirection.Normalize();
        enemyBody.AddForce(enemyDirection * CalculateJumpForce(distance,9.8f) * grapplePullEnemyForce, ForceMode.VelocityChange);
        
    }
    public void playerPullToWall()
    {
        collide.enabled = false;
        wallGrabbed = false;
        playerRB.AddForce(forceDirection * CalculateJumpForce(distanceToWall, 9.8f) * grapplePullToWallForce, ForceMode.VelocityChange);
        Debug.Log("force multiplier: " + CalculateJumpForce(distanceToWall, 9.8f));
    }

    private float CalculateJumpForce(float jumpHeight, float gravity)
    {
        float playerGravity = gravity + (gravity * player.GetComponent<playerController1>().gravityIncrease);
        //jumpheight -> distance to target to jump to
        return Mathf.Sqrt(2 * jumpHeight * playerGravity);
    }


}
