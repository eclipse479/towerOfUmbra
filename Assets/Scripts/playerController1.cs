﻿using UnityEngine;
using UnityEngine.UI;

public class playerController1 : MonoBehaviour
{
    [SerializeField] public LayerMask platformLayerMask;
    //health variables
    public Text healthText;
    public float maxHealth;
    private float currentHealth;


    //how much force a jump has
    public float jumpForce;
    //is the player on the ground
    private bool grounded;
    //can player double jump
    private bool doubleJump;
    
    //rigidbody
    private Rigidbody rb;
    //enemy collision knockback
    public float knockBackAmount;

    //teh sword
    public GameObject swordBase;
    public float swordSpeed; // how fast teh sword moves
    private bool swordSwinging; // is sword swinging
                                // Start is called before the first frame update
     //combo Counter
    public float maxCounterResetTimer;
    private float comboCounterResetTimer;
    private int hitCounter;
    public Text comboCounter;

    //pausing
    private bool paused;
    public Canvas pauseScreen;
    public Canvas gameplayMenu;

    //player has no health
    public Canvas deathScreen;
    private bool dead;

    //player health bar
    public GameObject healthBar;
    private Image healthbarImage;

    //temp player speed text
    public Text deleteThisLater;

    //player movement
    public float speed;
    public float playerMaxMovementSpeed;

    //variable for checking if player is grounded
    public float boxCastMaxDistance = 1;
    private RaycastHit boxHit;

    //collider that the physics material is on so friction can be changed
    private Collider collide;
    public float antiSlopeBumpForce = 0.75f;

    private RaycastHit slopeCheckRay;
    RaycastHit inFrontOfPlayer;
    private bool jumping;
    private bool applyAntiBumpForce;
    private float maxAntiBumpForceTimer = 0.3f;
    private float antiBumpForceTimer;
    void Start()
    {
        dead = false;
        paused = false;
        jumping = false;
        applyAntiBumpForce = false;
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

    // Update is called once per frame
    private void FixedUpdate()
    {
        //RB movement in fixed update
       
    }
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

                //input for the player movement

                if (Input.GetKey(KeyCode.RightArrow) && !swordSwinging || Input.GetKey(KeyCode.D) && !swordSwinging)
                {
                    //remove friction when running
                    collide.material.dynamicFriction = 0.0f;
                    collide.material.staticFriction = 0.0f;
                    collide.material.frictionCombine = PhysicMaterialCombine.Minimum;
                    //change player facing direction
                    transform.eulerAngles = new Vector3(0, -90, 0);
                    //move player
                    if(grounded)
                    rb.AddForce(transform.forward * speed * Time.deltaTime);
                    else
                        rb.AddForce(transform.forward * speed * Time.deltaTime * 0.5f);
                }
                if (Input.GetKey(KeyCode.LeftArrow) && !swordSwinging || Input.GetKey(KeyCode.A) && !swordSwinging)
                {
                    //remove friction when running
                    collide.material.dynamicFriction = 0.0f;
                    collide.material.staticFriction = 0.0f;
                    collide.material.frictionCombine = PhysicMaterialCombine.Minimum;
                    //change player facing direction
                    transform.eulerAngles = new Vector3(0, 90, 0);
                    //move player
                    if (grounded)//player movement on the ground
                        rb.AddForce(transform.forward * speed * Time.deltaTime);
                    else // slower acceleration while in the air
                        rb.AddForce(transform.forward * speed * Time.deltaTime * 0.75f);
                }
                if (Input.GetKeyDown(KeyCode.W) && grounded)
                {
                    //jumps
                    Vector3 velocityKill = new Vector3(0, -rb.velocity.y, 0);
                    rb.AddForce(velocityKill, ForceMode.Impulse);
                    rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                    jumping = true;
                    applyAntiBumpForce = false;
                }
                else if (Input.GetKeyDown(KeyCode.W) && doubleJump)
                {
                    //jumps if in the air (double jump)
                    Vector3 antiFall = new Vector3(0, -rb.velocity.y, 0);
                    rb.AddForce(antiFall, ForceMode.Impulse);
                    rb.AddForce(transform.up * jumpForce * 0.5f, ForceMode.Impulse);//jump half as high
                    doubleJump = false;
                }
                ///---------------------------------------------------------------------------------------------------------------------------
                //checks in front is the ground is different from the current ground then apply a downward force to keep player on the ground
                checkInFront();
                //check the ground beneth the player
                slopeCheck();
                //apply anti bump force for slopes
                applyAntiBump();
                //keeps the player speed in check
                speedCheck();
                ///---------------------------------------------------------------------------------------------------------------------------
                //when movement keys are released
                if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    changeFriction();
                }
                ///box cast to check if the player is grounded
                //box cast for if player is grounded and can jump
                if (Physics.BoxCast(transform.position, new Vector3(0.125f, 0.1f, 0.125f), -transform.up, out boxHit, Quaternion.identity, boxCastMaxDistance, platformLayerMask))
                {
                    grounded = true;
                    jumping = false;
                    doubleJump = true;
                    if(boxHit.collider.gameObject.tag == "Finish")
                    {
                        Renderer rend = boxHit.collider.gameObject.GetComponent<Renderer>();
                        rend.material.color = Color.red;
                    }
                }
                else
                {
                    grounded = false;
                }
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
        else if(dead)
        {
            playerIsDead();
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
        if(currentX < 90)
        {
            //swing sword
            swordBase.transform.Rotate(new Vector3(swordSpeed, 0, 0) * Time.deltaTime);
        }
        //reset sword to inactive state
        else if(currentX > 90)
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

        Vector3 knockBackDirection = direction + transform.up;
        //remove current velocity then knocks back player
        rb.velocity = Vector3.zero;
        //if on ground push off ground(so friction with floor is removed)
        if(grounded)
        {
            rb.AddForce(transform.up , ForceMode.Impulse);
        }
        rb.AddForce(knockBackDirection * knockBackAmount, ForceMode.Impulse);
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
        Time.timeScale = 0.0f;
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

    private void changeFriction()
    {
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            //resets friction on player

            collide.material.dynamicFriction = 1.0f;
            collide.material.staticFriction = 1.0f;
            collide.material.frictionCombine = PhysicMaterialCombine.Maximum;
        }
    }


    private void slopeCheck()
    {
        
        if (Physics.Raycast(transform.position + new Vector3(0,-0.1f,0), -transform.up, out slopeCheckRay, 1.0f, platformLayerMask))
        {
            Debug.DrawRay(transform.position + new Vector3(0, -0.1f, 0), -transform.up * slopeCheckRay.distance, Color.blue);
        }
        else
        {
            Debug.DrawRay(transform.position + new Vector3(0, -0.1f, 0), -transform.up, Color.blue);
        }
        ///---------------------------------------------------------------------------------------------------------
        //deleteThisLater.text = "Normal X: " + slopeCheckRay.normal.x + " Normal Y: " + slopeCheckRay.normal.y + " Normal Z: " + slopeCheckRay.normal.z;
        ///---------------------------------------------------------------------------------------------------------
    }

    private void applyAntiBump()
    {
        if (applyAntiBumpForce && !jumping)
        {
            deleteThisLater.text = "APPLY THE FORCE!!!";
            rb.AddForce(-Vector3.up * antiSlopeBumpForce);
            //timer for how long the force is applied if the player does nothing
            antiBumpForceTimer -= Time.deltaTime;
            if (antiBumpForceTimer < 0)
                applyAntiBumpForce = false;
        }
        else
        {
        }
    }
    /// <summary>
    /// sends a raycast down in front of the player and to determine is a slope if in front
    /// </summary>
    /// <returns></returns>
    private void checkInFront()
    {
        Vector3 rayCastPos = transform.position + new Vector3(0, -0.4f, 0) + (transform.forward * 0.1f);
        float length = 0.4f;
        if (Physics.Raycast(rayCastPos, -transform.up, out inFrontOfPlayer, length, platformLayerMask))
        {
            
            Debug.DrawRay(rayCastPos, -transform.up * inFrontOfPlayer.distance, Color.cyan);
            //if the spot in front of the player hits ground and the normal is not the same as the normal the player is on
            if (inFrontOfPlayer.normal.y != slopeCheckRay.normal.y)
            {
                antiBumpForceTimer = maxAntiBumpForceTimer;
                applyAntiBumpForce = true;
            }
            else
            {
                deleteThisLater.text = "sameGround";
            }
        }
        else
        {
            deleteThisLater.text = "nothing in front";
            Debug.DrawRay(rayCastPos, -transform.up * length, Color.cyan);
        }
    }
}
