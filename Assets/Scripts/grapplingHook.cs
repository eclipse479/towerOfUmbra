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
    [Tooltip("aditional height the grapple starts at")]
    public float baseHeightIncrease;

    private GameObject parent;
    private Rigidbody rb;
    #endregion

    #region player stats
    [Header("PLAYER STATS")]
    [Tooltip("the player")]
    public GameObject player;
    [Tooltip("the player left hand object")]
    public Transform JNT_Hand_L; 
    [Tooltip("the player left shoulder object")]
    public Transform JNT_UpperArm_L;
    private playerController1 control;
    [Tooltip("the camera that follows the player")]
    public Camera playerCamera;
    [Tooltip("how far the grapple is from the player")]
    public float grappleDistFromPlayer;
    private Rigidbody playerRB;
    #endregion


    
    #region forces
    private Collider collide;
    [Header("FORCE MULTIPLIERS")]
    [Tooltip("force multiplier when a wall is hit with the grapple")]
    public float grapplePullToWallForce;
    [Tooltip("force multiplier when an enemy is hit by the grapple")]
    public float grapplePullEnemyForce;
    #endregion

    #region spring stats
    [Header("SPRING")]
    public float startingDamper;
    public float startingSpring;
    public float startingMassScale;
    private SpringJoint spring;
    #endregion


    #region misc
    private LineRenderer lRend;
    private List<MeshRenderer> rends;

    private bool extending;
    private bool wallGrabbed;
    //is the hook extended
    private bool active;
    private bool retracting;

    private float distanceToWall;
    private float lerpPercent = 0;
    private float angle;

    //improved extending and retracting
    private Vector3 grappleStartingPos;
    private Vector3 maxExtendedPoint;
    private Vector3 grapplePoint;

    public float maxGrappleGrace;
    private float grappleGrace;
    private float pullTimer;
    #endregion

    #region particles
    private ParticleSystem impact;
    private Transform impactTransform;
    #endregion
    [HideInInspector]
    public GameObject grabbedEnemy;
    //vector for pulling direction player -> wall
    [HideInInspector]
    public Vector3 forceDirection;


    public Text deleteThisLater;
    void Start()
    {

        impact = ParticleManager.instance.addParticle("grappleHit", transform.position, transform.rotation);
        impactTransform = impact.gameObject.transform;


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
        pullTimer = 0.3f;
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

        if (spring)
        {
            transform.position = grapplePoint;
            if (Input.GetKeyDown(KeyCode.Space) && pullTimer < 0)
            {
                pullTimer = 0.4f;
                playerPullToWall();
            }
            if (!Input.GetMouseButton(1))//if the mouse button isn't being held then remove the spring from the grapple/turn swing off
            {
                stopGrapple();
            }
        }


        if (!active)
        {
            //gets a new spot to shoot a grapple towards
            if (grappleGrace > 0)
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
        else //if active
        {
            //reel in grapple if it is active
            if (!Input.GetMouseButton(1))
            {
                extending = false;
                if(extending)
                    StopCoroutine(extend());
                if (!retracting)
                    StartCoroutine(retract());         //start retracting when mouse button is let go
            }


            Vector3 input = transform.position - JNT_UpperArm_L.position;
            input.Normalize();
            if(player.transform.rotation.y < 0)
            {
                input.x *= -1;
            }
            control.animator().SetFloat("xAngle", input.x);
            control.animator().SetFloat("yAngle", input.y);
            string y = input.y.ToString("F2");
            string x = input.x.ToString("F2");
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
            angle = (Mathf.Atan2(grapplePoint.y - player.transform.position.y, grapplePoint.x - player.transform.position.x) * Mathf.Rad2Deg);
        }

        //stops the spaming of spacebar when grappled to a wall
        if(pullTimer > 0)
        pullTimer -= Time.deltaTime;
    }
    private void LateUpdate()
    {
        
        drawRope();
    }
    void drawRope()
    {
        if (active)
        {
            lRend.SetPosition(0, transform.position);
            lRend.SetPosition(1, JNT_Hand_L.position);
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
        //collisions with various objects with tags
        if (collision.gameObject.tag == "enemy" && extending)
        {
            impactTransform.position = gameObject.transform.position;
            impactTransform.rotation = gameObject.transform.rotation;
            impact.Play();
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
                impactTransform.position = gameObject.transform.position;
                impactTransform.rotation = gameObject.transform.rotation;
                impact.Play();
                SoundManager.instance.playSound("grappleHookImpact");
                extending = false;
                wallGrabbed = true;
                ContactPoint contact = collision.contacts[0];
                grapplePoint = contact.point;
                control.resetDoubleJump();
                startGrapple();
            }
        }
        else if(!wallGrabbed)
        {
            impactTransform.position = gameObject.transform.position;
            impactTransform.rotation = gameObject.transform.rotation;
            impact.Play();
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
        extending = true;
        lerpPercent = 0;                                //resest the lerp timer
        reappear();
        SoundManager.instance.playSound("grappleHookThrow");
        control.animator().SetBool("grappleThrow", true);
        StopCoroutine(retract());
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
        if(maxReached)
           yield return StartCoroutine(retract());         //start retracting when extending has been completed
    }

    IEnumerator retract()
    {
        retracting = true;
        lerpPercent = 0;                //reset the lerp value
        StopCoroutine(extend());
        SoundManager.instance.playSound("grappleHookReel");
        wallGrabbed = false;
        collide.enabled = false;        //collider turned off
        rb.isKinematic = true;          //kinematic to stop physics
        Vector3 theDis = transform.position - parent.transform.position;
        float dis = theDis.magnitude; ;
        while (retracting)
        {
            if(spring)
            {
                stopGrapple();
            }
            Vector3 endPos = parent.transform.position;

            transform.position = Vector3.MoveTowards(transform.position, endPos, extendRate * Time.deltaTime);
            lerpPercent += extendRate * Time.deltaTime;
            if (lerpPercent > dis)
            {
                retracting = false;
            }
            yield return null;
        }
        active = false;
        control.animator().SetBool("grappleThrow", false);
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

    public float theAngle()
    {
        return angle;
    }
}
