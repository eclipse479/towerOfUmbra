using System.Collections;
using UnityEngine;

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

    //improved extending and retracting
    private Vector3 grappleStartingPos;
    private float lerpPercent = 0;
    private Vector3 maxExtendedPoint;

    private bool retracting;
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
        grappleStartingPos = new Vector3(player.transform.position.x, player.transform.position.y + baseHeightIncrease, player.transform.position.z) + (playerCamera.transform.forward * grappleDistFromPlayer);
        
        //moves grapple to player position
        parent.transform.position = grappleStartingPos;

        //sets the line renderer to draw between the hook and parent
        lRend.SetPosition(0, grappleStartingPos);
        lRend.SetPosition(1, transform.position);

        if (!active)
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
                    StartCoroutine(extend());
                }
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
            extending = false;
            isEnemyGrabbed = true;
            pullEnemy(grabbedEnemy);
        }
        else if (collision.gameObject.tag == "grappleTarget" && !wallGrabbed && !isEnemyGrabbed && extending)
        {
            //grapple the wall
            playerRB.velocity = Vector3.zero;
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
    }

    private float CalculateJumpForce(float jumpHeight, float gravity)
    {
        float playerGravity = gravity + (gravity * player.GetComponent<playerController1>().gravityIncrease);
        //jumpheight -> distance to target to jump to
        return Mathf.Sqrt(2 * jumpHeight * playerGravity);
    }

    IEnumerator extend()
    {
        StopCoroutine(retract());
        rb.isKinematic = false;
        Vector3 startPos = parent.transform.position;
        maxExtendedPoint = parent.transform.position + (gameObject.transform.right * maxLength);
        while(extending)
        {
            transform.position = Vector3.MoveTowards(transform.position, maxExtendedPoint, extendRate * Time.deltaTime);
            //transform.position = Vector3.Lerp(startPos, maxExtendedPoint, lerpPercent);
            lerpPercent +=  extendRate * Time.deltaTime;

            if(lerpPercent > maxLength)
            {
                extending = false;
            }
            yield return null;
        }
        lerpPercent = 0;                                //resest the lerp timer
        extending = false;                              //will now start to retract
        yield return StartCoroutine(retract());         //start retracting when extending has been completed
    }

    IEnumerator retract()
    {
        StopCoroutine(extend());
        retracting = true;
        collide.enabled = false;        //collider turned off
        rb.isKinematic = true;          //kinematic to stop physics
        maxExtendedPoint = transform.position;
        while (retracting)
        {
            //retract the grappling hook
            //parent.transform.LookAt(gameObject.transform.position); // makes the hook face away from the player when retracting
            Vector3 endPos = parent.transform.position;
            //transform.position = Vector3.Lerp(maxExtendedPoint, endPos, lerpPercent);
            transform.position = Vector3.MoveTowards(transform.position, endPos, extendRate * Time.deltaTime);
            lerpPercent += extendRate * Time.deltaTime;
            if(lerpPercent > maxLength)
            {
                retracting = false;
                active = false;
            }
            yield return null;
        }
        lerpPercent = 0;                //reset the lerp value
    }
}
