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
    private float counterResetTimer;
    private int hitCounter;
    public Text comboCounter;

    //pausing
    private bool paused;
    public Canvas pauseScreen;
    public Canvas gameplayMenu;

    public Slider healthSlider;

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
        counterResetTimer = maxCounterResetTimer;

        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
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
            if (counterResetTimer > 0)
            {
                counterResetTimer -= Time.deltaTime;
            }
            else if (counterResetTimer < 0)
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
            if (Input.GetKeyDown(KeyCode.Space) && grounded || Input.GetKeyDown(KeyCode.W) && grounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
            else if (Input.GetKeyDown(KeyCode.Space) && doubleJump || Input.GetKeyDown(KeyCode.W) && doubleJump)
            {
                Vector3 antiFall = new Vector3(0, -rb.velocity.y, 0);
                rb.AddForce(antiFall);
                rb.AddForce(transform.up * jumpForce * 0.5f, ForceMode.Impulse);//jump half as high
                doubleJump = false;
            }


            RaycastHit boxHit;
            //box cast for if player is grounded and can jump
            if (Physics.BoxCast(transform.position + new Vector3(0, 0, 0), new Vector3(0, Bounds.size.y, 0), new Vector3(0, -1, 0), out boxHit, transform.rotation, 0.22f, platformLayerMask))
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
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            Gizmos.color = Color.red;
        }
            Gizmos.DrawWireCube(transform.position - new Vector3(0,0.22f,0), new Vector3(0.25f, 0.2f, 0.25f));
    }

    public void swordCollision()
    {
        hitCounter++;
        counterResetTimer = maxCounterResetTimer;
        comboCounter.text = "Combo: " + hitCounter;
    }

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
