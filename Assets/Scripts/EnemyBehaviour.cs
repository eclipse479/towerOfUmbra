using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    // Where the hit box is
    public Transform hit_transform;
    public float hit_range = 0.5f;
    public LayerMask attack_layer;
    RaycastHit hit;

    // Detecting obstacles and player
    public float detection_range = 5.0f; // How far the enemy can detect the player
    public float max_ray_dist = 10.0f; // When enemy can has seen the player.
    float current_ray_dist;
    public float speed = 5.0f;
    public Transform target; // The player
    Rigidbody rb;
    Ray ray;

    // Keep track of the player
    public float detect_distance = 5.0f;
    float distance_to_player;

    // Rays to check on each side
    Ray ledge_ray;
    const int num_of_rays = 2; // There will only ever be two
    public float ray_offset = 3.0f; // How far apart the rays are from the Player
    public float drop_cast_dist = 1.5f; // The 

    // Distance check for an obstacle
    public float min_dist = 1.0f;
    float drop_check;

    // Attacking
    bool is_attacking = false;
    float attack_timer;
    public float attack_duration = 2.0f;
    public float attack_range = 0.8f;


    // Placeholder for attack
    public GameObject sword;

    // Shooting needs
    public GameObject bullet;
    bool is_shooting = false;
    public float shoot_cooldown = 2.0f;
    float shoot_timer;

    // Switch for states
    bool is_alert;

    // Enemy Behaviour State
    STATE behaviour = STATE.WALKING;

    // Start is called before the first frame update
    void Start()
    {
        // Rigidbody and rays
        rb = GetComponent<Rigidbody>();
        ray = new Ray();

        // Get the player as target
        target = GameObject.Find("player").transform;

        // A Forward Raycast to see in front of itself
        current_ray_dist = max_ray_dist;

        // Downward ray cast to check its sides
        drop_check = drop_cast_dist;

        // They cast down no matter what
        ledge_ray = new Ray();
        ledge_ray.direction = -transform.up;

        // Attack Timer
        attack_timer = attack_duration;

        // Shoot Cooldown
        shoot_timer = shoot_cooldown;

        // Attack
        sword.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // The direction of the ray changes each frame
        ray.origin = transform.position;
        ray.direction = transform.right;
        Debug.DrawRay(ray.origin, ray.direction, Color.green);

        // Is target within attack range
        if (attackRange())
        {
            behaviour = STATE.ATTACK;
        }

        // Has detected player but is not within attack range
        if (detectionZone())
        {
            if (Physics.Raycast(ray.origin, target.position - transform.position, out hit, detection_range, attack_layer.value))
            {
                if (!attackRange())
                    behaviour = STATE.SHOOT;
                else
                    behaviour = STATE.ATTACK;
            }
        }

        // Check state to determine actions
        switch(behaviour)
        {
            case 0: // Walking or patroling
                walkForNothing();
                break;
            case (STATE)1: // Chasing
                moveToPlayer();
                break;
            case (STATE)2: // Attacking
                attack();
                break;
            case (STATE)3: // Shoot
                shoot();
                break;
            default:
                behaviour = STATE.WALKING;
                break;
        }

        // How far is the player from the enemy
        detectionZone();
     
        if (lineOfSight(ray))
        {
            behaviour = STATE.CHASING; 
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void shoot()
    {
        if (!is_shooting && shoot_timer == shoot_cooldown)
        {
            is_shooting = true;
            Instantiate(bullet, transform.position + (transform.right), transform.rotation);
        }

        if (is_shooting)
        {
            shoot_timer -= 2.0f * Time.deltaTime;
        }

        if (shoot_timer <= 0.0f)
        {
            shoot_timer = shoot_cooldown;
            is_shooting = false;
        }
    }

    /// <summary>
    /// The attacks when the player is within striking distance
    /// </summary>
    /// <returns></returns>
    bool attack()
    {
        if (!is_attacking)
        {
            is_attacking = true;
            sword.SetActive(true);
        }

        if (is_attacking)
        {
            // When it hit's something
            if (Physics.SphereCast(hit_transform.position, hit_range, hit_transform.right, out hit, hit_range, attack_layer.value))
            {
                // Is it the player?
                if (hit.collider.gameObject.layer == 8)
                {
                    Debug.Log("Enemy has hit" + hit.collider.gameObject.name);
                    is_attacking = false;
                    sword.SetActive(false);
                    attack_timer = attack_duration;
                    return true;
                }
            }
            else
                attack_timer -= 2.0f * Time.deltaTime;
                sword.transform.rotation =  Quaternion.Slerp(sword.transform.rotation, Quaternion.Euler(sword.transform.rotation.x, sword.transform.rotation.y, -65.0f), attack_timer/attack_duration);
                Debug.Log("Attack Length of Enemy: " + attack_timer.ToString());
        }
        else if (attack_timer <= 0.0f)
        {
            is_attacking = false;
            sword.SetActive(false);
            attack_timer = attack_duration;
        }
        return false;
    }


    /// <summary>
    /// Look in front of itself
    /// </summary>
    /// <param name="a_ray"></param>
    /// <returns> Is target in range? Hit an obstacle? </returns>
    bool lineOfSight(Ray a_ray)
    {
        if (Physics.Raycast(a_ray, out hit, current_ray_dist))
        {
            LayerMask layer_hit = hit.collider.gameObject.layer;

            switch (layer_hit)
            {
                // If it hits the player
                case 8:
                    current_ray_dist = (hit.point - ray.origin).magnitude;
                    /* Will add State change later */
                    return true;
                case 9:
                  
                    if ((hit.point - transform.position).magnitude < min_dist && behaviour == STATE.WALKING)
                    { 
                        transform.right *= -1; // Turn around
                        a_ray.direction = transform.right;      
                    }
                    return false;
                default:
                    current_ray_dist = (hit.point - ray.origin).magnitude;
                    return false;
            }
        }
        // Other set the ray length back to max
        current_ray_dist = max_ray_dist;
        return false;
    }

    /// <summary>
    /// For finding the player and determining if to face them
    /// </summary>
    bool detectionZone()
    {
        // Check where the player is
        distance_to_player = (target.position - transform.position).magnitude;

        if (distance_to_player < detect_distance)
        {
            transform.right = new Vector3((target.position - transform.position).x, 0,0).normalized;
            behaviour = STATE.CHASING;
            return true;
        }
        behaviour = STATE.WALKING;
        return false;
    }

    /// <summary>
    /// Move towards the player
    /// </summary>
    void moveToPlayer()
    {
        Vector2 move_velocity = (target.position - transform.position).normalized;
        
        rb.AddForce(move_velocity * speed * Time.deltaTime, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Walk back and forth since they got nothing better to do || Only walks when in WALKING State
    /// </summary>
    void walkForNothing()
    {
        // Update the downwards cast
        ledge_ray.origin = transform.position + (transform.right * ray_offset);
        Debug.DrawRay(ledge_ray.origin, ledge_ray.direction, Color.red);
        // If it doesn't hit anything
        if (!Physics.Raycast(ledge_ray, out hit, drop_cast_dist))
        {
            transform.right *= -1;
        }

        rb.AddForce(transform.right * speed * Time.deltaTime, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Melee attack range
    /// </summary>
    /// <returns> If target is in range </returns>
    bool attackRange()
    {
        return (transform.position - target.position).magnitude < attack_range;
    }

    void die()
    {
        Destroy(this);
    }

    // For the enemy's search zone
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detection_range);
    }

    // For the attack radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(hit_transform.position, hit_range);
    }



    public enum STATE
    {
        WALKING, // 0
        CHASING, // 1
        ATTACK,   // 2
        SHOOT   // 3
    }
}
