using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class grapplingHook : MonoBehaviour
{
    //total length of the hook
    [Header("GRAPPLE STATS")]
    [Tooltip("how far the grapple can move")]
    public float maxLength;
    [Tooltip("shortest possible grappling Hook")]
    [Min(0.1f)]
    public float minLength;
    [Tooltip("how fast the grapple can grow")]
    [Min(0.1f)]
    public float lengthenReelSpeed;
    [Tooltip("how fast the grapple can shrink")]
    [Min(0.1f)]
    public float shortenReelSpeed;
    //how fast it extends
    [Tooltip("how fast the grapple moves")]
    [Min(0.1f)]
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
    [Header("SPRING")]
    public float startingDamper;
    public float startingSpring;
    public float startingMassScale;
    private bool retracting;
    private List<MeshRenderer> rends;

    private float grappleGrace;
    public float maxGrappleGrace;

    private SpringJoint spring;
    private Vector3 grapplePoint;

    public Text deleteThisLater;
    // Start is called before the first frame update
    void Start()
    {
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

        if (Input.GetMouseButtonDown(1))
        {
            grappleGrace = maxGrappleGrace;
        }
        else
        {
            grappleGrace -= Time.deltaTime;
        }

        if (!active)
        {
            //gets a new spot to shoot a grapple towards
            if (grappleGrace >= 0)
            {
                RaycastHit hit;
                grappleGrace = -1;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    Vector3 destination = hit.point; // position clicked
                    //make grapple face position clicked to it can extend properly
                    parent.gameObject.transform.LookAt(new Vector3(destination.x, destination.y, transform.position.z));
                    collide.enabled = true;
                    active = true; // grapple exists
                    StartCoroutine(extend());
                }
            }
        }
        if(wallGrabbed)
        {
            transform.position = grapplePoint;
            if (Input.GetKey(KeyCode.W))
            {
                shortenGrapplingHook();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                lengthenGrapplingHook();
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            extending = false;
            StopCoroutine(extend());
            StartCoroutine(retract());         //start retracting when extending has been completed
            if (spring)
                stopGrapple();
        }
    }
    private void LateUpdate()
    {
        drawRope();
    }
    void drawRope()
    {

        lRend.SetPosition(0, transform.position);
        lRend.SetPosition(1, parent.transform.position);
    }

    private void startGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100.0f))
        {
            spring = player.gameObject.AddComponent<SpringJoint>();
            spring.autoConfigureConnectedAnchor = false;
            spring.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(parent.transform.position, grapplePoint);

            //distance grapple will try to stay at
            spring.maxDistance = distanceFromPoint * 0.8f;
            spring.minDistance = minLength;
            //change these at will:
            spring.damper = startingDamper;
            spring.spring = startingSpring;
            spring.massScale = startingMassScale;

            lRend.positionCount = 2;
        }
    }

    private void shortenGrapplingHook()
    {
        if (spring.maxDistance > minLength)
        {
            spring.maxDistance -= shortenReelSpeed * Time.deltaTime;
        }
        else
            spring.maxDistance = minLength;
    }

    private void lengthenGrapplingHook()
    {
        if (spring.maxDistance < maxLength)
        {
            spring.maxDistance += lengthenReelSpeed * Time.deltaTime;
        }
        else
            spring.maxDistance = maxLength;
    }
    private void stopGrapple()
    {
        Destroy(spring);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //collisions with various objects with tags
        if (collision.gameObject.tag == "enemy" && !isEnemyGrabbed && !wallGrabbed && extending)
        {
            //grapple the enemy
            grabbedEnemy = collision.gameObject;
            extending = false;
            isEnemyGrabbed = true;
            pullEnemy(grabbedEnemy);
        }
        else if (collision.gameObject.tag == "grappleTarget" && !wallGrabbed && !isEnemyGrabbed && extending)
        {
            wallGrabbed = true;
            extending = false;
            ContactPoint contact = collision.contacts[0];
            grapplePoint = contact.point;
            startGrapple();
            //forceDirection = (new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, player.gameObject.transform.position.z) - player.gameObject.transform.position);
            //distanceToWall = forceDirection.magnitude;//length
            //forceDirection.Normalize();
            //playerPullToWall();
        }
        else
        {
            //hits anything else
            extending = false;
            StopCoroutine(extend());
            StartCoroutine(retract());
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
        enemyBody.AddForce(enemyDirection * CalculateJumpForce(distance, 9.8f) * grapplePullEnemyForce, ForceMode.VelocityChange);

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
        reappear();
        StopCoroutine(retract());
        extending = true;
        rb.isKinematic = false;
        bool maxReached = false;
        maxExtendedPoint = parent.transform.position + (gameObject.transform.right * maxLength);
        while (extending)
        {
            transform.position = Vector3.MoveTowards(transform.position, maxExtendedPoint, extendRate * Time.deltaTime);
            //transform.position = Vector3.Lerp(startPos, maxExtendedPoint, lerpPercent);
            lerpPercent += extendRate * Time.deltaTime;

            if (lerpPercent >= maxLength)
            {
                extending = false;
                maxReached = true;
                Debug.Log("retract");
                
            }
            yield return null;
        }
        lerpPercent = 0;                                //resest the lerp timer
        if(maxReached)
           yield return StartCoroutine(retract());         //start retracting when extending has been completed
    }

    IEnumerator retract()
    {
        StopCoroutine(extend());
        wallGrabbed = false;
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
            if (lerpPercent > maxLength)
            {
                retracting = false;
            }
            yield return null;
        }
        lerpPercent = 0;                //reset the lerp value
        active = false;
        disappear();
    }

    private void disappear()
    {
        foreach(MeshRenderer rend in rends)
        {
            rend.enabled = false;
        }
    }

    private void reappear()
    {
        foreach (MeshRenderer rend in rends)
        {
            rend.enabled = true;
        }
    }
}
