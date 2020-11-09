using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public float bulletSpeed = 10;
    public float lifeTime = 7;

    // Identify player
    LayerMask player;

    // Player target
    public Transform target;

    // Rigidbody
    Rigidbody bullet_rb;

    private void Awake()
    {
        // Layer to damage
        player = 8;

        target = GameObject.FindGameObjectWithTag("player").transform;

        bullet_rb = GetComponent<Rigidbody>();
        Vector3 direction = ((target.position + new Vector3(0, 1.2f, 0)) - transform.position).normalized;
        transform.forward = direction;

        bullet_rb.AddForce(direction * bulletSpeed, ForceMode.VelocityChange);
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
            Destroy(gameObject);
        }
        else if (hit.layer != player && hit.layer != 11)
        {
            Destroy(gameObject);
        }
    }

}
