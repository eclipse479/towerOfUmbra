using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class playerController1 : MonoBehaviour
{
    [SerializeField] public LayerMask platformLayerMask;
    //health variables
    [Header("HEATH SETTINGS")]
    [Tooltip("Text that displays health left")]
    public Text healthText;
    [Tooltip("Player max health")]
    public float maxHealth;
    [HideInInspector]
    public float currentHealth;
    //player health bar
    [Tooltip("The health bar game object")]
    public GameObject healthBar;
    private Image healthbarImage;



    //rigidbody
    private Rigidbody rb;
    //enemy collision knockback
    [Header("KNOCKBACK")]
    [Tooltip("How high the player is knocked when hit")]
    public float verticalKnockBackAmount;
    [Tooltip("How far the player is knocked when hit")]
    public float horizontalKnockBackAmount;

    [Header("SWORD SETTINGS")]
    //the sword
    [Tooltip("the base of the players sword")]
    public GameObject swordBase;
    [Tooltip("how fast the sword swings")]
    public float swordSpeed;    // how fast the sword moves
    private bool swordSwinging; // is sword swinging
                          
    [Header("COMBO COUNTER")]
    [Tooltip("timer for the combo counter")]
    public float maxCounterResetTimer;
    private float comboCounterResetTimer;
    private int hitCounter;
    [Tooltip("combo counter text")]
    public Text comboCounter;

    //pausing
    private bool paused;
    [Header("UIS")]
    [Tooltip("the pause UI")]
    public Canvas pauseScreen;
    [Tooltip("the in game UI")]
    public Canvas gameplayMenu;
    //player has no health
    [Tooltip("the game over screen")]
    public Canvas deathScreen;
    
    private bool dead;


    [Header("PLAYER MOVEMENT")]
    //player movement
    [Tooltip("the players acceleration")]
    public float speed;
    [Tooltip("the players max horizontal movement speed")]
    public float playerMaxMovementSpeed;
    //how much force a jump has
    [Tooltip("How strong the player jumping is")]
    public float jumpForce;
    [Tooltip("movement force multiplier when the player is not grounded")]
    [Range(0,1)]
    public float airMovementMultiplier = 0.75f;
    [Tooltip("Force multiplier for the double jump, min - 0")]
    [Min(0)]
    public float doubleJumpForce;
    //is the player on the ground
    private bool grounded;
    //can player double jump
    private bool doubleJump;
    //is player jumping
    private bool jumping;
    [Tooltip("how long until the player can be grounded again after jumping")]
    public float maxGroundedDelay = 0.2f;
    private float groundedDelay;


    //collider that the physics material is on so friction can be changed
    private Collider collide;
    [Tooltip("force applied to keep the player on the ground at slopes")]
    public float antiSlopeBumpForce = 0.75f;



    //timer for how long the force is applied
    private float maxAntiBumpForceTimer = 0.3f;
    private float antiBumpForceTimer;
    [Header("OTHER")]
    [Tooltip("increase in gravity 0 -> normal 1 -> double")]
    public float gravityIncrease = 0;
    //variable for checking if player is grounded
    [Tooltip("how far the box cast is sent downward")]
    public float boxCastMaxDistance = 1;
    private RaycastHit boxHit;
    private RaycastHit floorCheckRay;
    RaycastHit inFrontOfPlayer;
    //temp player speed text
    [Tooltip("Text used for debugging")]
    public Text deleteThisLater;
    void Start()
    {
        dead = false;
        paused = false;
        jumping = false;
        pauseScreen.enabled = false;
        deathScreen.enabled = false;
        if (swordSpeed <= 0)
        {
            swordSpeed = 1;
        }
        //the rigidbody
        rb = GetComponent<Rigidbody>();
        //the collider
        collide = GetComponent<Collider>();
        //remaining health
        healthText.text = "Health: " + maxHealth;
        //is grounded
        grounded = true;
        //sword is not swinging
        swordSwinging = false;
        //combo counter
        comboCounterResetTimer = maxCounterResetTimer;

        //health bar values
        healthbarImage = healthBar.transform.GetChild(1).gameObject.GetComponent<Image>();
        currentHealth = maxHealth;

    }
    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * rb.mass * gravityIncrease);
    }
    // Update is called once per frame
    void Update()
    {
        //pausing
        if (!dead)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!paused)
                {
                    paused = true;
                    Time.timeScale = 0.0f;
                    pauseScreen.enabled = true;
                    gameplayMenu.enabled = false;
                }
                else
                {

                    paused = false;
                    Time.timeScale = 1.0f;
                    pauseScreen.enabled = false;
                    gameplayMenu.enabled = true;
                }
            }
            if (!paused)
            {
                //timer for the combo counter
                if (comboCounterResetTimer > 0)
                {
                    comboCounterResetTimer -= Time.deltaTime;
                }
                else if (comboCounterResetTimer < 0)
                {
                    hitCounter = 0;
                    comboCounter.text = "Combo: " + hitCounter;
                }
                //keeps the player speed in check
                speedCheck();
                //input for the player movement
                if (Input.GetKey(KeyCode.RightArrow) && !swordSwinging || Input.GetKey(KeyCode.D) && !swordSwinging)
                {
                    //remove friction when running
                    removeFriction();
                    //change player facing direction
                    transform.eulerAngles = new Vector3(0, -90, 0);
                    //move player
                    if (grounded)
                        rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.Force);
                    else
                        rb.AddForce(transform.forward * speed * Time.deltaTime * airMovementMultiplier, ForceMode.Force);
                }
                if (Input.GetKey(KeyCode.LeftArrow) && !swordSwinging || Input.GetKey(KeyCode.A) && !swordSwinging)
                {
                    //remove friction when running
                    removeFriction();
                    //change player facing direction
                    transform.eulerAngles = new Vector3(0, 90, 0);
                    //move player
                    if (grounded)//player movement on the ground
                        rb.AddForce(transform.forward * speed * Time.deltaTime,ForceMode.Force);
                    else // slower acceleration while in the air
                        rb.AddForce(transform.forward * speed * Time.deltaTime * airMovementMultiplier, ForceMode.Force);
                }


                if (Input.GetKeyDown(KeyCode.W) && grounded) //jumps
                {
                    //removes current vertical velocity
                    Vector3 velocityKill = rb.velocity;
                    velocityKill.y = 0;
                    rb.velocity = velocityKill;
                    //jumps
                    rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                    jumping = true;
                    antiBumpForceTimer = -1;
                    groundedDelay = maxGroundedDelay;
                }
                else if (Input.GetKeyDown(KeyCode.W) && doubleJump) //jumps if in the air (double jump)
                {
                    //removes current vertical velocity
                    Vector3 velocityKill = rb.velocity;
                    velocityKill.y = 0;
                    rb.velocity = velocityKill;
                    rb.AddForce(transform.up * jumpForce * doubleJumpForce, ForceMode.VelocityChange);//jump half as high
                    doubleJump = false;
                    jumping = true;
                    antiBumpForceTimer = -1;
                    groundedDelay = maxGroundedDelay;
                }
                
                //when movement keys are released
                if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    addFriction();
                }
                else // if any movement key is pressed
                {
                    if (rb.velocity.x > 1 || rb.velocity.x < -1)
                    {
                        //apply anti bump force for slopes
                        applyAntiBump();
                    }
                }
                ///box cast to check if the player is grounded
                //box cast for if player is grounded and can jump
                groundedDelay -= Time.deltaTime;
                if (Physics.BoxCast(transform.position, new Vector3(0.125f, 0.1f, 0.125f), -transform.up, out boxHit, Quaternion.identity, boxCastMaxDistance, platformLayerMask))
                {
                    grounded = true;
                    doubleJump = true;
                    if (groundedDelay < 0)
                    {
                        jumping = false;
                    }
                    if (boxHit.collider.gameObject.tag == "Finish")
                    {
                        //slope testing purposes
                        Renderer rend = boxHit.collider.gameObject.GetComponent<Renderer>();
                        rend.material.color = Color.red;
                    }
                }
                else
                {
                    grounded = false;
                }

                ///---------------------------------------------------------------------------------------------------------------------------
                //checks ground directally beneth and in front of the player
                groundCheck();
                ///---------------------------------------------------------------------------------------------------------------------------

                //swing sword
                if (Input.GetMouseButtonDown(0) && !swordSwinging)
                {
                    swordBase.transform.rotation = Quaternion.identity;
                    swordSwinging = true;
                    swordBase.SetActive(true);
                    swordBase.transform.eulerAngles = gameObject.transform.eulerAngles;
                }
                if (swordSwinging)
                {
                    swingSword();
                }
            }
        }
        else if (dead)
        {
            playerIsDead();
        }
        //debug ray to check if a ramp is infront of the player
        Debug.DrawRay(transform.position + new Vector3(0, -0.45f, 0), transform.forward * 0.25f, Color.green);
        if (jumping)
            deleteThisLater.color = Color.yellow;
        else if (antiBumpForceTimer > 0 && !jumping)
        {
            deleteThisLater.color = Color.red;
        }
        else
        {
            deleteThisLater.color = Color.cyan;
        }
    }


    /// <summary>
    /// swings the sword
    /// </summary>
    private void swingSword()
    {
        //activates the sword
        float currentX = swordBase.transform.rotation.eulerAngles.z; // x axis
        //should be replaced with swinging sword animation and turning on sword collider
        if (currentX < 90)
        {
            //swing sword
            swordBase.transform.Rotate(new Vector3(swordSpeed, 0, 0) * Time.deltaTime);
        }
        //reset sword to inactive state
        else if (currentX > 90)
        {
            //swordBase.transform.rotation = Quaternion.identity;
            swordBase.transform.eulerAngles = new Vector3(0, -90, 0);
            swordSwinging = false;
            swordBase.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "enemy" || collision.gameObject.tag == "bullet")
        {
            //reduce health
            currentHealth--;
            healthText.text = "Health: " + currentHealth;
            knockBack(collision.gameObject);
            //move health bar health
            healthbarImage.fillAmount = currentHealth / maxHealth;
            if (currentHealth <= 0)
            {
                dead = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        //Check if there has been a hit yet
        if (grounded)
        {
            Gizmos.color = Color.green;
            //Draw a cube that extends to where the hit exists 
            Gizmos.DrawWireCube(transform.position - transform.up * boxHit.distance, new Vector3(0.125f, 0.1f, 0.125f) * 2);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance 
        else
        {
            Gizmos.color = Color.red;
            //Draw a Ray forward from GameObject toward the maximum distance 
            Gizmos.DrawRay(transform.position, -transform.up * 0.22f);
            //Draw a cube at the maximum distance 
            Gizmos.DrawWireCube(transform.position - transform.up * boxCastMaxDistance, new Vector3(0.125f, 0.1f, 0.125f) * 2);
        }

    }
    /// <summary>
    /// when the sword collides with an enemy for combo purposes
    /// </summary>
    public void swordCollision()
    {
        hitCounter++;
        comboCounterResetTimer = maxCounterResetTimer;
        comboCounter.text = "Combo: " + hitCounter;
    }
    /// <summary>
    /// knockback applied to the player when colliding with an enemy
    /// </summary>
    /// <param name="enemy"></param>
    private void knockBack(GameObject enemy)
    {
        //determines direction to knock back
        float enemyX = enemy.transform.position.x;
        float playerX = transform.position.x;
        Vector3 direction = new Vector3(playerX - enemyX, 0, 0);

        direction.Normalize();

        //remove current velocity then knocks back player
        rb.velocity = Vector3.zero;

        Vector3 knockBackDirection = new Vector3(direction.x * horizontalKnockBackAmount, direction.y * verticalKnockBackAmount, 0);
        //get off ground so friction wont be taken into account
        if (grounded)
        {
            rb.AddForce(transform.up * 0.2f, ForceMode.VelocityChange);
        }
        rb.AddForce(knockBackDirection, ForceMode.VelocityChange);
    }
    /// <summary>
    /// player has run out of health and has died
    /// should play death animation ect.
    /// </summary>
    private void playerIsDead()
    {
        gameplayMenu.enabled = false;
        pauseScreen.enabled = false;
        deathScreen.enabled = true;
        addFriction();
    }
    /// <summary>
    /// checks the player speed and limits it if it exceeds the movement
    /// </summary>
    private void speedCheck()
    {
        if (rb.velocity.x > playerMaxMovementSpeed)
        {
            rb.velocity = new Vector3(playerMaxMovementSpeed, rb.velocity.y, rb.velocity.z);
        }
        else if (rb.velocity.x < -playerMaxMovementSpeed)
        {
            rb.velocity = new Vector3(-playerMaxMovementSpeed, rb.velocity.y, rb.velocity.z);
        }
    }
    /// <summary>
    /// increased friction to the highest possible value
    /// </summary>
    private void addFriction()
    {
            //resets friction on player
            collide.material.dynamicFriction = 1.0f;
            collide.material.staticFriction = 1.0f;
            collide.material.frictionCombine = PhysicMaterialCombine.Maximum;
    }
    private void removeFriction()
    {
        //resets friction on player
        collide.material.dynamicFriction = 0;
        collide.material.staticFriction = 0;
        collide.material.frictionCombine = PhysicMaterialCombine.Minimum;
    }

    /// <summary>
    /// checks the floor directally beneth the player
    /// </summary>
    private void floorCheck()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, -0.4f, 0), -transform.up, out floorCheckRay, 0.2f, platformLayerMask))
        {
            Debug.DrawRay(transform.position + new Vector3(0, -0.4f, 0), -transform.up * floorCheckRay.distance, Color.blue);
        }
        else
        {
            Debug.DrawRay(transform.position + new Vector3(0, -0.4f, 0), -transform.up * 0.2f, Color.blue);
        }
        ///---------------------------------------------------------------------------------------------------------
        //deleteThisLater.text = "Normal X: " + slopeCheckRay.normal.x + " Normal Y: " + slopeCheckRay.normal.y + " Normal Z: " + slopeCheckRay.normal.z;
        ///---------------------------------------------------------------------------------------------------------
    }

    /// <summary>
    /// applies a force downwards when enter/exiting a slope to keep the player on the ground
    /// </summary>
    private void applyAntiBump()
    {

        if (antiBumpForceTimer >= 0)
        {
            antiBumpForceTimer -= Time.deltaTime;
        }
        if (antiBumpForceTimer > 0 && !jumping /*&& grounded*/)
        {
            RaycastHit forwardRay;
            if (!Physics.Raycast(transform.position + new Vector3(0, -0.45f, 0), transform.forward, out forwardRay, 1.0f, platformLayerMask))
            {
                Debug.DrawRay(transform.position + new Vector3(0, -0.45f, 0), transform.forward, Color.black);
                deleteThisLater.text = "APPLY THE FORCE!!! time left: " + antiBumpForceTimer;
                rb.AddForce(-Vector3.up * antiSlopeBumpForce, ForceMode.VelocityChange);
            }
            else
                Debug.DrawRay(transform.position + new Vector3(0, -0.45f, 0), transform.forward * forwardRay.distance, Color.black);
        }
    }

    /// <summary>
    /// sends a raycast down in front of the player and to determine is a slope if in front
    /// </summary>
    /// <returns></returns>
    private void groundCheck()
    {
        //check directally beneth the player
        floorCheck();
        

        //check the ground slightly in front of the player
        Vector3 rayCastPos = transform.position + new Vector3(0, -0.4f, 0) + (transform.forward * 0.5f);
        float length = 0.4f;
        if (Physics.Raycast(rayCastPos, -transform.up, out inFrontOfPlayer, length, platformLayerMask))
        {
            Debug.DrawRay(rayCastPos, -transform.up * inFrontOfPlayer.distance, Color.cyan);
            //if the spot in front of the player hits ground and the normal is not the same as the normal the player is on
            if (floorCheckRay.normal.y != inFrontOfPlayer.normal.y && floorCheckRay.normal.y != 0 && inFrontOfPlayer.normal.y != 0)
            {
                antiBumpForceTimer = maxAntiBumpForceTimer;
            }
            else
            {
                deleteThisLater.text = "sameGround";
            }
        }
        else
        {
            deleteThisLater.text = "nothing in front";
            Debug.DrawRay(rayCastPos, -transform.up * length, Color.gray);
        }
    }

}
