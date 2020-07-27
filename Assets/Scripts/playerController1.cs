using UnityEngine;
using UnityEngine.UI;

public class playerController1 : MonoBehaviour
{
    [SerializeField] public LayerMask platformLayerMask;
    public Text healthText;
    public float maxHealth;
    private float currentHealth;
    public float speed;
    public float length;
    //how much force a jump has
    public float jumpForce;
    //is the player on the ground
    private bool grounded;
    //can player double jump
    private bool doubleJump;
    
    //locks rotation when swinging sword
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

    public GameObject healthBar;
    private Slider healthSlider;

    private float wireBoxHeight;

    private float boxCastMaxDistance = 1;
    private RaycastHit boxHit;
    void Start()
    {
        paused = false;
        pauseScreen.enabled = false;
        if (swordSpeed <= 0)
        {
            swordSpeed = 1;
        }
        //teh rigidbody
        rb = GetComponent<Rigidbody>();
        //remaining health
        healthText.text = "Health: " + maxHealth;
        //is grounded
        grounded = true;
        //sword is not swinging
        swordSwinging = false;
        //combo counter
        comboCounterResetTimer = maxCounterResetTimer;

        healthSlider = healthBar.GetComponent<Slider>();
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;



        Collider collide = gameObject.GetComponent<Collider>();
        Vector3 temp = collide.bounds.size;
        wireBoxHeight = temp.y;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //RB movement in fixed update
       
    }
    void Update()
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
                transform.eulerAngles = new Vector3(0, -90, 0);
                rb.AddForce(transform.forward * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftArrow) && !swordSwinging || Input.GetKey(KeyCode.A) && !swordSwinging)
            {
                transform.eulerAngles = new Vector3(0, 90, 0);
                rb.AddForce(transform.forward * speed * Time.deltaTime);
            }
            if (Input.GetKeyDown(KeyCode.W) && grounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
            else if (Input.GetKeyDown(KeyCode.W) && doubleJump)
            {
                Vector3 antiFall = new Vector3(0, -rb.velocity.y, 0);
                rb.AddForce(antiFall);
                rb.AddForce(transform.up * jumpForce * 0.5f, ForceMode.Impulse);//jump half as high
                doubleJump = false;
            }

            ///box cast to check if the player is grounded
            //box cast for if player is grounded and can jump
            if (Physics.BoxCast(transform.position, new Vector3(0.125f, 0.1f, 0.125f), -transform.up, out boxHit, Quaternion.identity, boxCastMaxDistance, platformLayerMask))
            {
                grounded = true;
                doubleJump = true;
            }
            else
            {
                Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * length, Color.red);
                grounded = false;
            }
            //swing sword
            if (Input.GetMouseButtonDown(1) && !swordSwinging)
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
            if (collision.gameObject.tag == "enemy")
            {
                currentHealth--;
                healthText.text = "Health: " + currentHealth;
                knockBack(collision.gameObject);
            healthSlider.value = currentHealth;
                if (currentHealth <= 0)
                {
                    Debug.Log("ded");
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
        float enemyX = enemy.transform.position.x;
        float playerX = transform.position.x;

        Vector3 direction = new Vector3(playerX - enemyX, 0, 0);
        direction.Normalize();
        Vector3 knockBackDirection;

        knockBackDirection = direction + transform.up;

        rb.AddForce(knockBackDirection * knockBackAmount, ForceMode.Impulse);
    }
}
