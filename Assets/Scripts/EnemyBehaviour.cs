/*
 * Script: Enemy Behaviour
 * Author: Nixon Sok
 * 
 * Purpose: A state machine that dictates the enemy's actions and affects enemy counter
 * 
 * Defieciencies: Doesn't work with NavMesh yet.
 * 
 * 
 */



using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour
{
    // Knockback
    [Header("Knockback to self")]
    public float knockback = 20.0f;
    private Transform healthBar;
    private Slider healthSlider;
    [Header("Health Points")]
    public int health;

    // Where the hit box is
    [Header("Attack Area and Settings")]
    public Transform hit_box;
    public float hit_range = 0.5f;
    public LayerMask attack_layer;
    RaycastHit hit;

    // Detecting obstacles and player
    [Header("Enemy Detection Settings")]
    public float detection_range = 5.0f; // How far the enemy can detect the player
    public float max_ray_dist = 10.0f; // When enemy can has seen the player.
    float current_ray_dist;
    
    [Header("Movement Speed")]
    public float speed = 5.0f;

    [Header("Target to chase")]
    public Transform target; // The player

    Rigidbody rb;
    Ray ray;

    [Header("Centre of Enemy")]
    public Transform ray_centre;

    // Keep track of the player
    public float detect_distance = 5.0f;
    float distance_to_player;

    // Rays to check on each side
    Ray ledge_ray;
    const int num_of_rays = 2; // There will only ever be two

    [Header("Ledge detection")]
    public float ray_offset = 3.0f; // How far apart the rays are from the Player
    public float drop_cast_dist = 1.5f; // The 

    // Distance check for an obstacle
    [Header("Obstacle Check Distance")]
    public float min_dist = 1.0f;
    float drop_check;

    // Attacking
    bool is_attacking = false;
    bool can_attack = true;
    float attack_timer;
    [Header("Attack Settings")]
    public float attack_cooldown = 2.0f; [Tooltip("Timer before it can attack again")]
    public float attack_range = 0.8f; [Tooltip("Close-range attack area")]

    // Shooting needs
    [Header("Shooting Settings")]
    public GameObject bullet;
    public float shoot_cooldown = 2.0f;
    bool is_shooting = false;
    bool can_shoot = true;
    float shoot_timer;

    // Switch for states
    bool is_alert;

    //reference to enemies left text
    [Header("The enemy counter text")]
    public Text enemiesLeftText;
    private EnemiesLeftCounter textCounter;
    // Enemy Behaviour State
    STATE behaviour = STATE.WALKING;

    // Stun values
    [Header("Stun settings")]
    public float stun_duration = 2.0f;
    public float stun_recovery = 2.0f;
    bool is_stunned = true;
    float stun_time;

    // Things that need to be loaded before first frame
    private void Awake()
    {
        // Get the player as target
        target = GameObject.FindGameObjectWithTag("player").transform;
        rb = GetComponent<Rigidbody>();

        // Rigidbody and rays
        ray = new Ray();

        if (ray_centre != null)
        {
            ray.origin = ray_centre.position;
        }

        // They cast down no matter what
        ledge_ray = new Ray();
        ledge_ray.direction = -transform.up;

        Transform healthBarCanvas = gameObject.transform.Find("healthBarCanvas");
        healthBar = healthBarCanvas.gameObject.transform.Find("healthBar");
        healthSlider = healthBar.GetComponent<Slider>();
        healthSlider.maxValue = health;
        healthSlider.value = health;

        // Attack Timer
        attack_timer = attack_cooldown;

        // Shoot Cooldown
        shoot_timer = shoot_cooldown;

        // Stun duration
        stun_time = stun_duration;
    }

    // Start is called before the first frame update
    void Start()
    {
        textCounter = enemiesLeftText.GetComponent<EnemiesLeftCounter>();
        textCounter.add();
      
        // A Forward Raycast to see in front of itself
        current_ray_dist = max_ray_dist;

        // Downward ray cast to check its sides
        drop_check = drop_cast_dist;

     
    }

    // Update is called once per frame
    void Update()
    {
        // The direction of the ray changes each frame
        if (ray_centre != null)
        {
            ray.origin = ray_centre.position;
        }
        else
        {
            ray.origin = transform.position;
        }

        ray.direction = transform.forward;
        Debug.DrawRay(ray.origin, ray.direction, Color.green);

        // If it has no health points
        if (health <= 0.0f)
        {
            die();
        }

        // As long as the enemy isn't stunned, do it's thing.
        if (!is_stunned)
        {
              // Has detected player but is not within attack range
              if (detectionZone())
              {
                  if (Physics.Raycast(ray.origin, target.position - transform.position, out hit, detection_range, attack_layer.value))
                  {
                      // Is the player isn't in melee range
                      if (!attackRange())
                      {
                          behaviour = STATE.SHOOT;
                      }
                      else
                      {
                          behaviour = STATE.ATTACK;
                      }
                  }
              }
              else
              {
                  behaviour = STATE.WALKING;
              }

              //// Check state to determine actions
              switch (behaviour)
              {
                  case 0: // Walking or patroling
                      walkForNothing();
                      break;
                  case (STATE)1: // Chasing
                      moveToPlayer();
                      break;
                  case (STATE)2:
                      attack();
                      break;
                  default:
                      break;
              }

              // Check in front of itself for obstacles or player
              lineOfSight(ray);

              // If the enemy has already attacked
              if (!can_shoot)
              {
                  shoot_timer -= 1 * Time.deltaTime;
                  if (shoot_timer <= 0.0f)
                  {
                      shoot_timer = shoot_cooldown;
                      can_shoot = true;
                  }
              }
        }
        else
        {
            stun_time -= stun_recovery * Time.deltaTime;
            // Return to normal when recovered
            if (stun_time <= 0.0f)
            {
                is_stunned = false;
                stun_time = stun_duration;
            }
        }
    }

    /// <summary>
    /// Play shooting animation the fire
    /// </summary>
    void shoot()
    {
        if (ray_centre != null)
            Instantiate(bullet, ray_centre.position + transform.forward, transform.rotation);
        else
            Instantiate(bullet, transform.position + transform.forward, transform.rotation);   
    }

    // For the animator events to reset shooting timer
    void isShooting()
    {
        is_shooting = false;
    }

    public bool shootTimer()
    {
        if (shoot_timer == shoot_cooldown)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// The attacks when the player is within striking distance
    /// </summary>
    void attack()
    {
        if (is_attacking)
        {
          // Check it it hits the player
          if (Physics.SphereCast(hit_box.position, hit_range, hit_box.forward, out hit, hit_range, attack_layer))
          {
                GameObject player = hit.collider.gameObject;
                Rigidbody player_rb = player.GetComponent<Rigidbody>();

                player_rb.AddForce((player.transform.up + -player.transform.forward) * knockback, ForceMode.Impulse);

                Debug.Log("Enemy has hit");
          }
        }
    }

    // Allow animator to disable attack
    void setAttack()
    {
           is_attacking = false;
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
                        transform.forward *= -1; // Turn around
                        a_ray.direction = transform.forward;
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
            transform.forward = new Vector3((target.position - transform.position).x, 0, 0).normalized;
            behaviour = STATE.CHASING;
            return true;
        }
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
        if (ray_centre != null)
            ledge_ray.origin = ray_centre.position + (transform.forward * ray_offset);
        else
            ledge_ray.origin = transform.position + (transform.forward * ray_offset);

        // In case ledge ray isn't pointing down
        // ledge_ray.direction = -transform.up;

        // If it doesn't hit anything
        if (!Physics.Raycast(ledge_ray, out hit, drop_cast_dist))
        {
            transform.forward *= -1;
        }

        rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.VelocityChange);
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
        textCounter.subtract();
        Destroy(gameObject);
    }

    // For the enemy's search zone
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detection_range);
    }

    // For the attack radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(hit_box.transform.position, hit_range);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "swordBlade")
        {
            is_stunned = true;

            // Reset Enemy velocity
            rb.velocity = Vector3.zero;

            rb.AddForce(-transform.forward  * knockback, ForceMode.Impulse);
            health--;
            healthSlider.value = health;
        }
    }

    public STATE State
    {
        get { return behaviour; }
    }

    // Return the shoot available
    public bool canShoot
    {
        get { return can_shoot; }
    }

    // When the A.I triggers the shooting animation trigger
    public bool IsShooting
    {
         get { return is_shooting; }
        set { is_shooting = value; }
    }

    // Return the shooting cooldown 
    public float shootCooldown
    {
        get { return shoot_timer; }
    }

    // Can the A.I attacking
    public bool canAttack
    {
        get { return can_attack; }
    }
    
    // Is it attacking?
    public bool isAttacking
    {
        get { return is_attacking; }
        set { is_attacking = value; }
    }

    public bool isStunned
    {
        get { return is_stunned; }
    }

    public enum STATE
    {
        WALKING, // 0
        CHASING, // 1
        ATTACK,   // 2
        SHOOT   // 3
    }
}
