using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public float bulletSpeed;
    public float lifeTime = 7;

    // Identify player
    LayerMask player; 

    // Rigidbody
    Rigidbody bullet_rb;

    // Start is called before the first frame update
    void Start()
    {
        // Layer to damage
        player = 8;

        bullet_rb = GetComponent<Rigidbody>();

        bullet_rb.AddForce(transform.right * bulletSpeed * Time.deltaTime, ForceMode.VelocityChange);
        // bulletSpeed = 8;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        // bullet_rb.AddForce(transform.right * bulletSpeed * Time.deltaTime, ForceMode.Force);


        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hit = collision.gameObject;

        if (hit.layer == player)
        {
            hit.GetComponent<Rigidbody>().AddForce(bullet_rb.velocity * 5, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }

}
