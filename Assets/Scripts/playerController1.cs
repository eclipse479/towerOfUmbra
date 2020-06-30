using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController1 : MonoBehaviour
{
    [SerializeField] public LayerMask platformLayerMask;
    public Text healthText;
    public float health;
    public float speed;
    public float length;
    public float jumpForce;
    private bool grounded;
    private bool doubleJump;
    private Rigidbody rb;


    RaycastHit m_Hit;
    // Start is called before the first frame update
    // Nixon: The Change
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        healthText.text = "Health: " + health;
        grounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.eulerAngles = new Vector3(0, -90, 0);
            rb.AddForce(transform.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.eulerAngles = new Vector3(0, 90, 0);
            rb.AddForce(transform.forward * speed * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.Space) && grounded || Input.GetKeyDown(KeyCode.W) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
        else if(Input.GetKeyDown(KeyCode.Space) && doubleJump || Input.GetKeyDown(KeyCode.W) && doubleJump)
        {
            Vector3 antiFall = new Vector3(0,-rb.velocity.y,0);
            rb.AddForce(antiFall);
            rb.AddForce(transform.up * jumpForce * 0.5f);//jump half as high
            doubleJump = false;
        }


        //box cast
        if (Physics.BoxCast(transform.position - new Vector3(0, -5.5f, 0), new Vector3(0.1f, -0.5f, 0.1f), -transform.up, out m_Hit, transform.rotation, 0.1f, platformLayerMask))
        {
            //Output the name of the Collider your Box hit
            grounded = true;
            doubleJump = true;
        }
        else
        {
            Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * length, Color.red);
            grounded = false;
        }

        //raycast
        RaycastHit ground;
        if (Physics.Raycast(gameObject.transform.position,new Vector3(0,-1,0),out ground, length, platformLayerMask))
        {
            Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * ground.distance, Color.green);
            grounded = true;
            doubleJump = true;
        }
        else
        {
            Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * length, Color.red);
            grounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "enemy")
        {
        health--;
        healthText.text = "Health: " + health;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Check if there has been a hit yet
        if (grounded)
        {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, transform.forward * m_Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position + transform.forward * m_Hit.distance, transform.localScale);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, transform.forward * 0.1f);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(transform.position + transform.forward * 0.1f, transform.localScale);
        }
    }
}
