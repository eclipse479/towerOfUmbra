using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class grapplingHook : MonoBehaviour
{
    #region grappleStats
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
    [Tooltip("aditional height the grapple starts at")]
    public float baseHeightIncrease;

    #endregion
    //has the hook hit an enemy
    //the player
    #region player stats
    [Header("PLAYER STATS")]
    [Tooltip("the player")]
    public GameObject player;
    private playerController1 control;
    [Tooltip("the camera that follows the player")]
    public Camera playerCamera;
    [Tooltip("how far the grapple is from the player")]
    public float grappleDistFromPlayer;
    #endregion
    private GameObject parent;


    private Rigidbody playerRB;
    private Rigidbody rb;
    private bool extending;
    private bool wallGrabbed;
    //enemy hit by grappling hook
    
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
    #region spring stats
    [Header("SPRING")]
    public float startingDamper;
    public float startingSpring;
    public float startingMassScale;
    private SpringJoint spring;
    #endregion
    private bool retracting;
    private List<MeshRenderer> rends;

    private float grappleGrace;
    public float maxGrappleGrace;

    private Vector3 grapplePoint;

    [HideInInspector]
    public GameObject grabbedEnemy;
    //vector for pulling direction player -> wall
    [HideInInspector]
    public Vector3 forceDirection;
    public Text deleteThisLater;
    // Start is called before the first frame update
    void Start()
    {
        control = player.GetComponent<playerController1>();
        rends = new List<MeshRenderer>();
        for (int i = 1; i <= 3; i++)
        {
            rends.Add(gameObject.transform.GetChild(i).GetComponent<MeshRenderer>());
        }
        for (int i = 0; i < 3; i++)
        {
            rends.Add(gameObject.transform.GetChild(0).gameObject.transform.GetChild(i).GetComponent<MeshRenderer>());
        }
        parent = gameObject.transform.parent.gameObject;
        lRend = transform.GetComponent<LineRenderer>();
        collide = gameObject.GetComponent<Collider>();
        rb = transform.GetComponent<Rigidbody>();
        playerRB = player.GetComponent<Rigidbody>();
        collide.enabled = false;
        extending = false;
        active = false;
        grappleGrace = -1;
        disappear();
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

        if(grappleGrace > 0)
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
                    parent.transform.LookAt(new Vector3(destination.x, destination.y, transform.position.z));
                    collide.enabled = true;
                    active = true; // grapple exists
                    StartCoroutine(extend());
                }
            }
        }
        if(wallGrabbed)
        {
            if (Input.GetKey(KeyCode.W))
            {
                shortenGrapplingHook();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                lengthenGrapplingHook();
            }
        }


        if(spring)
        {
            transform.position = grapplePoint;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerPullToWall();
            }
            if (!Input.GetMouseButton(1))//if the mouse button isn't being held then remove the spring from the grapple/turn swing off
            {
                stopGrapple();
            }
        }

        if(Input.GetMouseButtonUp(1))
        {
                extending = false;
                StopCoroutine(extend());
            if(!retracting)
                StartCoroutine(retract());         //start retracting when mouse button is let go
        }
    }
    private void LateUpdate()
    {
        
        drawRope();
    }
    void drawRope()
    {
        if (extending || wallGrabbed || retracting)
        {
            lRend.SetPosition(0, transform.position);
            lRend.SetPosition(1, parent.transform.position);
        }
        else
        {
            lRend.SetPosition(0, transform.position);
            lRend.SetPosition(1, transform.position);
        }
    }

    private void startGrapple()
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
        control.setGrappled(true);
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
        control.setGrappled(false);
        Destroy(spring);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
        //collisions with various objects with tags
        if (collision.gameObject.tag == "enemy" && extending)
        {
            extending = false;
            StopCoroutine(extend());
            if (!retracting)
                StartCoroutine(retract());
            //grapple the enemy
            grabbedEnemy = collision.gameObject;
            pullEnemy(grabbedEnemy);
        }
        else if (collision.gameObject.tag == "grappleTarget" )
        {
            if (extending)
            {
                extending = false;
                wallGrabbed = true;
                ContactPoint contact = collision.contacts[0];
                grapplePoint = contact.point;
                Debug.Log(grapplePoint);
                startGrapple();
            }
        }
        else
        {
            //hits anything else
            extending = false;
            StopCoroutine(extend());
            if (!retracting)
                StartCoroutine(retract());
        }
    }

    public void pullEnemy(GameObject thingToPull)
    {
        collide.enabled = false;
        Rigidbody enemyBody;
        enemyBody = thingToPull.GetComponent<Rigidbody>();
        Vector3 enemyDirection = (player.transform.position - thingToPull.transform.position);
        float distance = enemyDirection.magnitude;
        enemyDirection.Normalize();
        enemyBody.AddForce(enemyDirection * CalculateJumpForce(distance, 9.8f) * grapplePullEnemyForce, ForceMode.VelocityChange);

    }
    public void playerPullToWall()
    {
        forceDirection = (new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, player.gameObject.transform.position.z) - player.gameObject.transform.position);
        distanceToWall = forceDirection.magnitude;//length
        forceDirection.Normalize();

        collide.enabled = false;
        playerRB.AddForce(forceDirection * CalculateJumpForce(distanceToWall, 9.8f) * grapplePullToWallForce, ForceMode.VelocityChange);
    }

    private float CalculateJumpForce(float jumpHeight, float gravity)
    {
        float playerGravity = gravity + (gravity * control.gravityIncrease);
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
            if(spring)
            {
                stopGrapple();
            }
            Vector3 endPos = parent.transform.position;

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
        transform.position = parent.transform.position;
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
