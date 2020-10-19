using System.Collections;
using UnityEngine;
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
    [HideInInspector]
    public Image healthbarImage;



    //rigidbody
    private Rigidbody rb;
    //enemy collision knockback
    [Header("KNOCKBACK")]
    [Tooltip("How high the player is knocked when hit")]
    public float verticalKnockBackAmount;
    [Tooltip("How far the player is knocked when hit")]
    public float horizontalKnockBackAmount;
    [Tooltip("how long after getting hit can the player not move")]
    public float maxKnockBackNoMovementTimer;
    private float knockBackNoMovementTimer;

    [Header("INVINCIBILITY FRAMES")]
    [Tooltip("How many times the player will flash")]
    [Min(0)]
    public int numOfFlashes; 
    [Tooltip("How long each flash is")]
    public float flashLength;
    private GameObject playerRend;

    [Header("ATTACK SETTINGS")]
    [Tooltip("the maximum amount of time between attack clicks to do the next attack")]
    public float maxComboDelay;
    private float currentComboDelay;

    [HideInInspector]
    public int attackNumber;

    [Tooltip("how fast the sword swings")]
    public float swordSpeed;    // how fast the sword moves


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
    [Tooltip("movement force multiplier when the player is not grounded")]
    [Range(0,1)]
    public float airMovementMultiplier = 0.75f;
    [Tooltip("force applied to keep the player on the ground at slopes")]
    public float antiSlopeBumpForce = 0.75f;

    [Header("JUMPING")]
    [Tooltip("How strong the player jumping is")]
    public float jumpForce;
    [Tooltip("Force multiplier for the double jump, min - 0")]
    [Min(0)]
    public float doubleJumpForce;
    [Tooltip("Time when the player can jump after falling off a platform withour using double jump")]
    [Min(0.1f)]
    public float maxJumpHoldTime;
    private float jumpHoldTime = -1;
    [Tooltip("Time when the player can press the jump button while in the air and then jump when landed")]
    [Min(0.1f)]
    public float maxJumpBuffer;
    private float jumpBuffer = -1;
    private float groundedDelay;
    //is the player on the ground
    private bool grounded;
    //can player double jump
    private bool doubleJump;
    //is player jumping
    private bool jumping;


    //collider that the physics material is on so friction can be changed
    private Collider collide;



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
    [HideInInspector]
    public bool isGrappled;

    //animations
    private Animator ani;
    //private SoundManager soundManager;
    private void Awake()
    {
        isGrappled = false;
        //health bar values
        healthbarImage = healthBar.transform.GetChild(1).gameObject.GetComponent<Image>();
        if(playerStats.health <= 0 && !dead)
        {
           playerStats.health = maxHealth;
        }
        else
        {
           currentHealth = playerStats.health;
           healthbarImage.fillAmount = playerStats.health / maxHealth;
        }

        playerRend = gameObject.transform.GetChild(0).transform.GetChild(0).gameObject;
        //soundManager = FindObjectOfType<SoundManager>();
        
        //line to play a sound from anywhere
        //FindObjectOfType<SoundManager>().playSound("soundName");
    }
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
        attackNumber = 0;
        //the rigidbody
        rb = GetComponent<Rigidbody>();
        //the collider
        collide = GetComponent<Collider>();
        //remaining health
        healthText.text = "Health: " + playerStats.health;
        //is grounded
        grounded = true;
        //sword is not swinging


        ani = GetComponentInChildren<Animator>();
    }
    void FixedUpdate()
    {
        //increase in gravity for th eplayer
        rb.AddForce(Physics.gravity * rb.mass * gravityIncrease, ForceMode.Force);
        if (!dead && knockBackNoMovementTimer <= 0)
        {
            //movement
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                //move player
                if (grounded)
                    rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.VelocityChange);
                else
                    rb.AddForce(transform.forward * speed * Time.deltaTime * airMovementMultiplier, ForceMode.VelocityChange);
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                //move player
                if (grounded)//player movement on the ground
                    rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.VelocityChange);
                else // slower acceleration while in the air
                    rb.AddForce(transform.forward * speed * Time.deltaTime * airMovementMultiplier, ForceMode.VelocityChange);
            }
            if (jumpHoldTime >= 0 && jumpBuffer >= 0 && !isGrappled)
            {
                //removes current vertical velocity
                ani.SetTrigger("jumped"); // jump animation
                Vector3 velocityKill = rb.velocity;
                velocityKill.y = 0;
                rb.velocity = velocityKill;
                //jumps
                rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                jumping = true;
                antiBumpForceTimer = -1;
                jumpHoldTime = -1;  // -> not grounded
                jumpBuffer = -1;   // -> hasn't pressed the key
            }
            else if (jumpBuffer >= 0 && doubleJump && !isGrappled)
            {
                Vector3 velocityKill = rb.velocity;
                velocityKill.y = 0;
                rb.velocity = velocityKill;
                rb.AddForce(transform.up * jumpForce * doubleJumpForce, ForceMode.VelocityChange);//jump half as high
                doubleJump = false;
                jumping = true;
                antiBumpForceTimer = -1;
                jumpBuffer = -1;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        //pausing
        if (!dead)
        {
            //pausing
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
            //make sure game isn't paused for logic
            if (!paused)
            {
                timersUpdate();
                //keeps the player speed in check
                speedCheck();
                //input for the player movement
               
                    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    {
                        if (Input.GetKeyDown(KeyCode.D) && rb.velocity.x > 0)
                        {
                            //insert turn around animation call
                            rb.velocity = new Vector3(rb.velocity.x * 0.3f, rb.velocity.y, rb.velocity.z);
                        }
                        //remove friction when running
                        removeFriction();
                        //change player facing direction
                        transform.eulerAngles = new Vector3(0, -90, 0);
                    }
                    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    {
                        if (Input.GetKeyDown(KeyCode.A) && rb.velocity.x < 0)
                        {
                            //insert turn around animation call
                            rb.velocity = new Vector3(rb.velocity.x * 0.3f, rb.velocity.y, rb.velocity.z);
                        }
                        //remove friction when running
                        removeFriction();
                        //change player facing direction
                        transform.eulerAngles = new Vector3(0, 90, 0);
                    }
                    if (Input.GetKeyDown(KeyCode.Space)) //jumps
                    {
                        jumpBuffer = maxJumpBuffer;
                    }
                //check if falling
                if (!grounded && rb.velocity.y < 0)
                {
                    ani.SetBool("falling", true);
                }

                //controlled jumping -> allows short hops when button is tapped and large jumps when held
                if(Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0 && !isGrappled)
                {
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, rb.velocity.z);
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
                if (Physics.BoxCast(transform.position + new Vector3(0, 1.1f, 0), new Vector3(0.125f, 0.1f, 0.125f), -transform.up, out boxHit, Quaternion.identity, boxCastMaxDistance, platformLayerMask))
                {
                    grounded = true;
                    doubleJump = true;
                    ani.SetBool("grounded", true);
                    ani.SetBool("falling", false);
                    jumpHoldTime = maxJumpHoldTime;
                    if (groundedDelay < 0)
                    {
                        jumping = false;
                    }
                    //debugging, allows certain platforms to turn red when touching
                    if (boxHit.collider.gameObject.tag == "Finish")
                    {
                        //slope testing purposes
                        Renderer rend = boxHit.collider.gameObject.GetComponent<Renderer>();
                        rend.material.color = Color.red;
                    }
                }
                else // not on the ground
                {
                    grounded = false;
                    ani.SetBool("grounded", false);
                }

                ///---------------------------------------------------------------------------------------------------------------------------
                //checks ground directally beneth and in front of the player
                groundCheck();
                ///---------------------------------------------------------------------------------------------------------------------------

                //swing sword
                if (Input.GetMouseButtonDown(0) && grounded)
                {
                    if(attackNumber < 3)
                    {
                        currentComboDelay = maxComboDelay; // check if animation is playing
                    }
                    attackNumber++;
                    if(attackNumber == 1) //starts the first attack the rest should occur automatically if clicked again
                    {
                        swingSword("firstAttack");
                        Debug.Log("start");
                    }
                }
                if(currentComboDelay >= 0)
                {
                    currentComboDelay -= Time.deltaTime; //count frames
                }
                else if(currentComboDelay < 0)
                {
                    ani.SetBool("attacking", false);
                    if(attackNumber == 1 || attackNumber == 2)
                    attackNumber = 0;//reset attack when no more frames
                }
                deleteThisLater.text = attackNumber.ToString();
            }
        }
        else if (dead)
        {
            playerIsDead();
        }
        //debug ray to check if a ramp is infront of the player
        Debug.DrawRay(transform.position + new Vector3(0, -0.45f, 0), transform.forward * 0.25f, Color.green);

       // if (jumping)
       //     deleteThisLater.color = Color.yellow;
       // else if (antiBumpForceTimer > 0 && !jumping)
       // {
       //     deleteThisLater.color = Color.red;
       // }
       // else
       // {
       //     deleteThisLater.color = Color.blue;
       // }
        if (currentComboDelay > 0)
            deleteThisLater.color = Color.blue;
        else
        {
            deleteThisLater.color = Color.red;
        }
    }
    

    /// <summary>
    /// swings the sword
    /// </summary>
    /// </summary>e
    private void swingSword(string attackName)
    {
        ani.SetBool("attacking", true);
        
        ani.SetBool(attackName, true); // attack animation
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "enemy" || collision.gameObject.tag == "bullet")
        {
            Debug.Log(collision.gameObject.name);
            //reduce health
            playerStats.health--;
                
            healthText.text = "Health: " + playerStats.health;
            knockBack(collision.gameObject);
            //move health bar health
            healthbarImage.fillAmount = playerStats.health / maxHealth;
            
            if (playerStats.health <= 0)
            {
                //*insert death animation*
                if(!dead)
                {
                    ani.SetTrigger("dead");
                    dead = true;
                }
                gameObject.layer = 19;
            }
            else
            {
                StartCoroutine(Flasher());
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
            Gizmos.DrawWireCube(transform.position - (transform.up * boxHit.distance) + new Vector3(0,1.1f,0), new Vector3(0.125f, 0.1f, 0.125f) * 2);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance 
        else
        {
            Gizmos.color = Color.red;
            //Draw a Ray forward from GameObject toward the maximum distance 
            Gizmos.DrawRay(transform.position, -transform.up * 0.22f);
            //Draw a cube at the maximum distance 
            Gizmos.DrawWireCube(transform.position - (transform.up * boxCastMaxDistance) + new Vector3(0, 1.1f, 0), new Vector3(0.125f, 0.1f, 0.125f) * 2);

        }

    }
    /// <summary>
    /// when the sword collides with an enemy for combo purposes
    /// </summary>
    public void swordCollision()
    {
        Debug.Log("sword hit a thing");
    }
    /// <summary>
    /// knockback applied to the player when colliding with an enemy
    /// </summary>
    /// <param name="enemy"></param>

    private void knockBack(GameObject enemy)
    {
        float xDirection = horizontalKnockBackAmount;
        //determines direction to knock back
        float enemyX = enemy.transform.position.x;
        float playerX = transform.position.x;
        knockBackNoMovementTimer = maxKnockBackNoMovementTimer;
        //remove current velocity then knocks back player
        rb.velocity = Vector3.zero;

        if (playerX - enemyX < 0)
        {
            xDirection *= -1; //makes sure the player is always knocked in away from the thing they hit
        }
        if (grounded)
        {
            rb.AddForce(transform.up * 0.1f, ForceMode.VelocityChange);
        }
        rb.AddForce(new Vector3(xDirection,verticalKnockBackAmount,0), ForceMode.VelocityChange);
    }

    private void timersUpdate()
    {
        if(jumpHoldTime >= 0)
            jumpHoldTime -= Time.deltaTime;
        if(jumpBuffer >= 0)
            jumpBuffer -= Time.deltaTime;
        if(knockBackNoMovementTimer >= 0)
            knockBackNoMovementTimer -= Time.deltaTime;
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
        //set value for idle/walking/running animation(they have been blended together)
        float currentSpeed = rb.velocity.x;
        if (currentSpeed < 0)
        {
            currentSpeed *= -1; // always positive
        }
        float speedInput = currentSpeed / playerMaxMovementSpeed;
        ani.SetFloat("speed", speedInput);
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
        if (Input.GetKeyUp(KeyCode.A) && grounded && !Input.GetKey(KeyCode.D) || Input.GetKeyUp(KeyCode.D) && grounded && !Input.GetKey(KeyCode.A))
        {
            if (rb.velocity.x > 3 || rb.velocity.x < -3)
            {
                rb.velocity = new Vector3(rb.velocity.x * 0.8f, rb.velocity.y, rb.velocity.z);
            }
        }
    }
    private void removeFriction()
    {
        //resets friction on player
        collide.material.dynamicFriction = 0;
        collide.material.staticFriction = 0;
        collide.material.frictionCombine = PhysicMaterialCombine.Minimum;
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
                //deleteThisLater.text = "APPLY THE FORCE!!! time left: " + antiBumpForceTimer;
                rb.AddForce(-Vector3.up * antiSlopeBumpForce, ForceMode.VelocityChange);
            }
            else
                Debug.DrawRay(transform.position + new Vector3(0, -0.45f, 0), transform.forward * forwardRay.distance, Color.black);
        }
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
               // deleteThisLater.text = "sameGround";
            }
        }
        else
        {
            //deleteThisLater.text = "nothing in front";
            Debug.DrawRay(rayCastPos, -transform.up * length, Color.gray);
        }
    }

    /// <summary>
    /// player flash to signify Iframes
    /// </summary>
    IEnumerator Flasher()
    {
        gameObject.layer = 19;
        for (int i = 0; i < numOfFlashes; i++)
        {
            playerRend.SetActive(false);
            yield return new WaitForSeconds(flashLength);
            playerRend.SetActive(true);
            yield return new WaitForSeconds(flashLength);
        }
        gameObject.layer = 8;
    }
}
