using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class playerController1 : MonoBehaviour
{
    [SerializeField] public LayerMask platformLayerMask;
    //health variables
    #region helath settings
    [Header("HEATH SETTINGS")]
    [Tooltip("Player max health")]
    public float maxHealth;
    [HideInInspector]
    public float currentHealth;
    //player health bar
    [Tooltip("The health bar game object")]
    public GameObject healthBar;
    [HideInInspector]
    public Image healthbarImage;
    #endregion
    //enemy collision knockback
    #region knockback
    [Header("KNOCKBACK")]
    [Tooltip("How high the player is knocked when hit")]
    public float verticalKnockBackAmount;
    [Tooltip("How far the player is knocked when hit")]
    public float horizontalKnockBackAmount;
    [Tooltip("how long after getting hit can the player not move")]
    public float maxKnockBackNoMovementTimer;
    private float knockBackNoMovementTimer;
    #endregion
    #region Invincibility frames
    [Header("INVINCIBILITY FRAMES")]
    [Tooltip("How many times the player will flash")]
    [Min(0)]
    public int numOfFlashes; 
    [Tooltip("How long each flash is")]
    public float flashLength;
    private GameObject playerRend;
    private GameObject playerSecondRend;
    
    
    #endregion
    #region attack settings
    [Header("ATTACK SETTINGS")]
    [Tooltip("the maximum amount of time between attack clicks to do the next attack")]
    public float maxComboDelay;
    [HideInInspector]
    public float currentComboDelay;

    [HideInInspector]
    public int attackNumber;
    #endregion


    #region UIs
    [Header("UIS")]
    [Tooltip("the pause UI")]
    public Canvas pauseScreen;
    [Tooltip("the in game UI")]
    public Canvas gameplayMenu;
    //player has no health
    [Tooltip("the game over screen")]
    public Canvas deathScreen;
    #endregion

    #region player movement
    [Header("PLAYER MOVEMENT")]
    //player movement
    [Tooltip("the players acceleration")]
    public float acceleration;
    [Tooltip("the players max horizontal movement speed")]
    public float playerMaxMovementSpeed;
    //how much force a jump has
    [Tooltip("movement force multiplier when the player is not grounded")]
    [Range(0,1)]
    public float airMovementMultiplier = 0.75f;
    [Tooltip("force applied to keep the player on the ground at slopes")]
    public float antiSlopeBumpForce = 0.75f;
    [Tooltip("movement force multiplier when the player is grappled to a target")]
    [Range(0, 1)]
    public float maxGrappledMovementMultiplier = 0.75f;
    [Tooltip("rate at whcih the grappled timer is reduced")]
    public float grappleMovementTimerReductionSpeed;
    private float currentGrappleMovement;
    #endregion
    //timer for how long the force is applied
    private float maxAntiBumpForceTimer = 0.3f;
    private float antiBumpForceTimer;

    #region jumping
    [Header("JUMPING")]
    [Tooltip("How strong the player jumping is")]
    public float jumpForce;
    [Tooltip("Force multiplier for the double jump, min - 0")]
    [Min(0)]
    public float doubleJumpForce;
    [Tooltip("Time when the player can jump after falling off a platform without using double jump")]
    [Min(0.1f)]
    public float maxCoyoteTime;
    private float coyoteTime = -1;
    [Tooltip("Time when the player can press the jump button while in the air and then jump when landed")]
    [Min(0.1f)]
    public float maxJumpBuffer;
    private float jumpBuffer = -1;
    //can player double jump
    private bool doubleJump;
    //is player jumping
    private bool jumping;
    #endregion



    #region misc
    [Header("OTHER")]
    [Tooltip("increase in gravity 0 -> normal 1 -> double")]
    public float gravityIncrease = 0;
    //variable for checking if player is grounded
    [Tooltip("how far the box cast is sent downward")]
    public float boxCastMaxDistance = 1;
    private RaycastHit boxHit;
    private RaycastHit floorCheckRay;
    RaycastHit inFrontOfPlayer;
    [HideInInspector]
    public bool mainMenu;

    public grapplingHook hook;
    #endregion

    #region private objects
    //collider that the physics material is on so friction can be changed
    private Collider collide;
    //rigidbody
    private Rigidbody rb;
    //animator
    private Animator ani;
    #endregion


    private ParticleSystem bloodSplatter;
    private Transform particleTransform;
    //pausing
    private bool paused;
    private bool dead;
    private float groundedDelay;
    /// testing
    private float angle;

    //is the player on the ground
    private bool grounded;

    [HideInInspector]
    public bool isGrappled;
    [HideInInspector]
    public float speedInput;
    //temp player speed text
    [Tooltip("Text used for debugging")]
    public Text deleteThisLater;
    //private SoundManager soundManager;
    private void Awake()
    {
        bloodSplatter = ParticleManager.instance.addParticle("PlayerBloodSplatter", transform.position, transform.rotation);
        particleTransform = bloodSplatter.gameObject.transform;

        currentGrappleMovement = maxGrappledMovementMultiplier;
        isGrappled = false;
        //health bar values
        healthbarImage = healthBar.transform.GetChild(1).gameObject.GetComponent<Image>();
        if(playerStats.health <= 0 && !dead)
        {
            playerStats.health = maxHealth;
            playerStats.maxHealth = maxHealth;
        }
        else
        {
           currentHealth = playerStats.health;
           healthbarImage.fillAmount = playerStats.health / maxHealth;
        }

        playerRend = gameObject.transform.GetChild(0).transform.GetChild(0).gameObject;
        playerSecondRend = gameObject.transform.GetChild(0).transform.GetChild(2).gameObject;
        //soundManager = FindObjectOfType<SoundManager>();
        
        //line to play a sound from anywhere
        //SoundManager.instance.playSound("soundName");
    }
    void Start()
    {
        dead = false;
        pauseScreen.enabled = false;
        deathScreen.enabled = false;
        attackNumber = 0;
        //the rigidbody
        rb = GetComponent<Rigidbody>();
        //the collider
        collide = GetComponent<Collider>();
        //animator
        ani = GetComponentInChildren<Animator>();
    }
    void FixedUpdate()
    {
        if(!paused)
        {
            //increase in gravity for the player
            rb.AddForce(Physics.gravity * rb.mass * gravityIncrease, ForceMode.Force);
            if (!dead && knockBackNoMovementTimer <= 0)
            {
                
                if (grounded)
                {
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
                    {
                        rb.AddForce(transform.forward * acceleration * Time.deltaTime, ForceMode.VelocityChange);
                    }
                }
                else if(isGrappled)
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        if (hook.theAngle() >= 0 && hook.theAngle() < 100)//left of grapple point
                        {
                            //if moving left and on the right of the grapple point
                            currentGrappleMovement = maxGrappledMovementMultiplier;
                            rb.AddForce(transform.forward * acceleration * Time.deltaTime * currentGrappleMovement, ForceMode.VelocityChange);
                        }
                        else if (hook.theAngle() >= 80 && hook.theAngle() < 180)//right of grapple point
                        {
                            rb.AddForce(transform.forward * acceleration * Time.deltaTime * currentGrappleMovement, ForceMode.VelocityChange);
                        }
                        else // if above the grapple point
                        {
                            rb.AddForce(transform.forward * acceleration * Time.deltaTime * maxGrappledMovementMultiplier, ForceMode.VelocityChange);
                        }
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        if (hook.theAngle() >= 80 && hook.theAngle() < 180)//right of grapple point
                        {
                            //if moveing right and on the left of the grapple point
                            currentGrappleMovement = maxGrappledMovementMultiplier;
                            rb.AddForce(transform.forward * acceleration * Time.deltaTime * currentGrappleMovement, ForceMode.VelocityChange);
                        }
                        else if (hook.theAngle() >= 0 && hook.theAngle() < 100)//left of grapple point
                        {
                            rb.AddForce(transform.forward * acceleration * Time.deltaTime * currentGrappleMovement, ForceMode.VelocityChange);
                        }
                        else // if above the grapple point
                        {
                            rb.AddForce(transform.forward * acceleration * Time.deltaTime * maxGrappledMovementMultiplier, ForceMode.VelocityChange);
                        }
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
                    {
                        rb.AddForce(transform.forward * acceleration * Time.deltaTime * airMovementMultiplier, ForceMode.VelocityChange);
                    }
                }
               
                if (coyoteTime >= 0 && jumpBuffer >= 0 && !isGrappled)
                {
                    //removes current vertical velocity
                    ani.SetTrigger("jumped"); // jump animation
                    if (rb.velocity.y < 0)
                    {
                        Vector3 velocityKill = rb.velocity;
                        velocityKill.y = 0;
                        rb.velocity = velocityKill;
                    }
                    //jumps
                    rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                    jumping = true;
                    antiBumpForceTimer = -1;
                    coyoteTime = -1;  // -> not grounded
                    jumpBuffer = -1;   // -> hasn't pressed the key
                }
                else if (jumpBuffer >= 0 && doubleJump && !isGrappled)
                {
                    ani.SetTrigger("jumped"); // jump animation
                    if (rb.velocity.y < 0)
                    {
                        Vector3 velocityKill = rb.velocity;
                        velocityKill.y = 0;
                        rb.velocity = velocityKill;
                    }
                    rb.AddForce(transform.up * jumpForce * doubleJumpForce, ForceMode.VelocityChange);//jump half as high
                    doubleJump = false;
                    jumping = true;
                    antiBumpForceTimer = -1;
                    jumpBuffer = -1;
                }
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
            if (Input.GetKeyDown(KeyCode.Escape) & !mainMenu)
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
               
                    if (Input.GetKey(KeyCode.D))
                    {
                        if (Input.GetKeyDown(KeyCode.D) && rb.velocity.x > 0)
                        {
                                rb.velocity = new Vector3(rb.velocity.x * 0.3f, rb.velocity.y, rb.velocity.z);
                        }
                        //remove friction when running
                        removeFriction();
                        //change player facing direction
                        transform.eulerAngles = new Vector3(0, -90, 0);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        if (Input.GetKeyDown(KeyCode.A) && rb.velocity.x < 0)
                        {
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
               // else // if any movement key is pressed
               // {
               //     if (rb.velocity.x > 1 || rb.velocity.x < -1)
               //     {
               //         //apply anti bump force for slopes
               //         applyAntiBump();
               //     }
               // }
                ///box cast to check if the player is grounded
                //box cast for if player is grounded and can jump
                if (groundedDelay > 0)
                groundedDelay -= Time.deltaTime;
                if (Physics.BoxCast(transform.position + new Vector3(0, 1.1f, 0), new Vector3(0.125f, 0.1f, 0.125f), -transform.up, out boxHit, Quaternion.identity, boxCastMaxDistance, platformLayerMask))
                {
                    grounded = true;
                    doubleJump = true;
                    ani.SetBool("grounded", true);
                    ani.SetBool("falling", false);
                    coyoteTime = maxCoyoteTime;
                    if (groundedDelay <= 0)
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

                //swing sword
                if (Input.GetMouseButtonDown(0) && !isGrappled)
                {
                        resetComboCooldown();
                        attackNumber++;
                        attackNumber = Mathf.Clamp(attackNumber, 0, 3);
                }    
                if(attackNumber == 1) //starts the first attack the rest should occur automatically if clicked again
                {
                    swingSword("firstAttack");
                }
                
                if(currentComboDelay >= 0)
                {
                    currentComboDelay -= Time.deltaTime;
                }
                else if(currentComboDelay < 0)
                {
                    if(attackNumber > 0)
                    {
                        attackNumber = 0;
                    }
                    //resets the attacks
                    ani.SetBool("attacking", false);
                    ani.SetBool("firstAttack", false);
                    ani.SetBool("secondAttack", false);
                    ani.SetBool("thirdAttack", false);
                }
                
            }
        }
        else if (dead)
        {
            playerIsDead();
        }
        //debug ray to check if a ramp is infront of the player
        Debug.DrawRay(transform.position + new Vector3(0, -0.45f, 0), transform.forward * 0.25f, Color.green);
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
        if (collision.gameObject.tag == "bullet")
        {
            //reduce health
            takeDamage(1.0f);
            knockBack(collision.gameObject);
            
            
        }
        if (playerStats.health <= 0)
        {
            if (!dead)
            {
                ani.SetTrigger("dead");
                dead = true;
            }
            gameObject.layer = 19;
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

    /// <summary>
    /// knockback applied to the player when colliding with an enemy
    /// </summary>
    /// <param name="enemy"></param>

    public void knockBack(GameObject enemy)
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
        if(coyoteTime >= 0)
            coyoteTime -= Time.deltaTime;
        if(jumpBuffer >= 0)
            jumpBuffer -= Time.deltaTime;
        if(knockBackNoMovementTimer >= 0)
            knockBackNoMovementTimer -= Time.deltaTime;
        if (currentGrappleMovement > 0)
        {
            currentGrappleMovement -= Time.deltaTime * grappleMovementTimerReductionSpeed;
            if (currentGrappleMovement < 0)
                currentGrappleMovement = 0;
        }
        
        
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
        speedInput = currentSpeed / playerMaxMovementSpeed;
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

    public void flashStart()
    {
        StartCoroutine(Flasher());
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
            playerSecondRend.SetActive(false);
            yield return new WaitForSeconds(flashLength);
            playerRend.SetActive(true);
            playerSecondRend.SetActive(true);
            yield return new WaitForSeconds(flashLength);
        }
        gameObject.layer = 8;
    }

    public void resetComboCooldown()
    {
        currentComboDelay = maxComboDelay;
    }

    public void setGrappled(bool grappled)
    {
        isGrappled = grappled;
        ani.SetBool("grappled", grappled);
    }

    public void setMainmenu(bool newBool)
    {
        paused = newBool;
        mainMenu = newBool;
        Time.timeScale = 1.0f;
    }

    private void grappleTimerSet()
    {
        angle = hook.theAngle();
        
        if (angle > 90)
            angle = 180 - angle;

        angle -= 90;
        angle *= -1;
    }
    public void takeDamage(float damage)
    {
        playerStats.health -= damage;
        healthbarImage.fillAmount = playerStats.health / maxHealth;
        particleTransform.position = gameObject.transform.position + new Vector3(0, 1, 0);
        particleTransform.rotation = gameObject.transform.rotation;
        bloodSplatter.Play();

        if (playerStats.health <= 0)
        {
            if (!dead)
            {
                ani.SetTrigger("dead");
                SoundManager.instance.playSound("playerDeath");
                dead = true;
            }
            gameObject.layer = 19;
        }
        else
        {
            SoundManager.instance.playSound("playerDamaged_1");
            StartCoroutine(Flasher());
        }
    }

    public Animator animator()
    {
        return ani;
    }
    public void resetDoubleJump()
    {
        doubleJump = true;
    }
    public void playFootstep()
    {
        if (speedInput > 0.1f && !jumping)
        {
            SoundManager.instance.playSound("footstep_1");
        }
    }
}
