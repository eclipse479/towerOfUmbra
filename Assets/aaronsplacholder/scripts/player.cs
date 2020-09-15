using UnityEngine;
using UnityEngine.UI;

public class player : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 50.0f;
    private Animator ani;
    public float maxSpeed;
    public bool grounded;
    RaycastHit boxHit;
    public float boxCastMaxDistance;
    public float jumpPower;
    [SerializeField] public LayerMask platformLayerMask;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ani = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(transform.forward * speed * Time.deltaTime,ForceMode.VelocityChange);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
    void Update()
    {
        speedCheck();
        //movement
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            //ani.SetLayerWeight(1, 0);
            rb.velocity = new Vector3(0,rb.velocity.y,0);
        }
        if(!grounded && rb.velocity.y < 0)
        {
            ani.SetBool("falling", true);
        }
        if (Input.GetKey(KeyCode.A))//move left
        {
            transform.eulerAngles = new Vector3(0, -90, 0);
        }
        if (Input.GetKey(KeyCode.D))//move right
        {
            transform.eulerAngles = new Vector3(0, 90, 0);
        } 
        //jump
        if (Input.GetKeyDown(KeyCode.W) && grounded)
        {
            ani.SetTrigger("jumped"); // jump animation
            rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
        //attack
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ani.SetTrigger("attack"); // attack animation
        }
        //grapple ripoff

        //is player grounded
        if (Physics.BoxCast(transform.position + new Vector3(0,1.1f,0), new Vector3(0.125f, 0.1f, 0.125f), -transform.up, out boxHit, Quaternion.identity, boxCastMaxDistance, platformLayerMask))
        {
            grounded = true;
            ani.SetBool("grounded", true);
            ani.SetBool("falling", false);
        }
        else
        {
            grounded = false;
            ani.SetBool("grounded", false);
        }
       
        speedCalabrate();
    }

    private void speedCheck()
    {
        if (rb.velocity.x > maxSpeed)
        {
            rb.velocity = new Vector3(maxSpeed, rb.velocity.y, rb.velocity.z);
        }
        else if (rb.velocity.x < -maxSpeed)
        {
            rb.velocity = new Vector3(-maxSpeed, rb.velocity.y, rb.velocity.z);
        }
    }
    void OnDrawGizmos()
    {
        //Check if there has been a hit yet
        if (grounded)
        {
            Gizmos.color = Color.green;
            //Draw a cube that extends to where the hit exists 
            Gizmos.DrawWireCube(transform.position + new Vector3(0, 1.1f, 0) - transform.up * boxHit.distance, new Vector3(0.125f, 0.1f, 0.125f) * 2);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance 
        else
        {
            Gizmos.color = Color.red;
            //Draw a Ray forward from GameObject toward the maximum distance 
            Gizmos.DrawRay(transform.position, -transform.up * 0.22f);
            //Draw a cube at the maximum distance 
            Gizmos.DrawWireCube(transform.position + new Vector3(0, 1.1f, 0) - transform.up * boxCastMaxDistance, new Vector3(0.125f, 0.1f, 0.125f) * 2);
        }

    }

    private void speedCalabrate()
    {
        float currentSpeed = rb.velocity.x;
        if(currentSpeed < 0)
        {
            currentSpeed *= -1; // always positive
        }
        float speedInput = currentSpeed / maxSpeed;
        ani.SetFloat("speed", speedInput);
    }
}
