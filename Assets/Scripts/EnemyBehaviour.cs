/*
 * Script: Enemy Behaviour
 * Author: Nixon Sok
 * 
 * Purpose: A state machine that dictates the enemy's actions and affects enemy counter
 * 
 * Defieciencies: Doesn't account for grounded check
 * 
 * 
 */



using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour
{
    private Animator animator;

    // Knockback
    [Header("Knockback to self")]
    public float knockback_horizontal = 20.0f;
    public float knockback_vertical = 10.0f;


    [Header("Knockback to Sword")]
    public float knockback_to_player_horizontal = 20.0f;
    public float knockback_to_player_vertical = 10.0f;

    private Transform healthBar;
    private Slider healthSlider;

    [Header("Health Points")]
    public int health;
    int max_health;

    // Where the hit box is
    [Header("Attack Area and Settings")]
    [Tooltip("The Gameobject used to attack the player")]public Transform hit_box;
    [Tooltip("How much area can the attack cover")] public float hit_range = 0.5f;
    [Tooltip("Damage value against player each hit")]public float damage_to_player = 1.0f;
    [Tooltip("Set to player, no questions")] public LayerMask attack_layer;
    RaycastHit hit;

    // Detecting obstacles and player
    [Header("Enemy Detection Settings")]
    [Tooltip("The detection range of the enemy")]public float detection_range = 5.0f; // How far the enemy can detect the player
    [Tooltip("Max line of sight")] public float max_ray_dist = 10.0f; // When enemy can has seen the player.
    float current_ray_dist; // Used as a dynamic raycast distance variable
    
    [Header("Movement Speed")]
    public float speed = 5.0f;
    [Tooltip("Adjust so the enemy can or can't move faster than a certain pace")] public float max_speed = 10.0f; 

    [Header("Target to chase")]
    [Tooltip("The target enemy chases")] public Transform target; // The player

    Rigidbody rb;
    Ray ray;

    [Header("Centre of Enemy")]
    [Tooltip("Either the root node or whatever object is called the ray centre")] public Transform ray_centre;

    // Keep track of the player
    float distance_to_player;

    // Rays to check on each side
    Ray ledge_ray;
    const int num_of_rays = 2; // There will only ever be two

    [Header("Ledge detection")]
    [Tooltip("How far the check for a ledge is placed in front of itself")] public float ray_offset = 3.0f; // How far apart the rays are from the Player
    [Tooltip("How far down it checks for when there's drop or not")] public float drop_cast_dist = 1.5f; // The 
    float drop_check;
    // Distance check for an obstacle
    [Header("Obstacle Check Distance")]
    [Tooltip("The minimum distance the enemy can see in front of them")]public float min_dist = 1.0f;
   

    // Attacking
    bool is_attacking = false;
    bool can_attack = true;
    float attack_timer;
    [Header("Attack Settings")]
    [Tooltip("Timer before it can attack again")]  public float attack_cooldown = 2.0f;
    [Tooltip("Close-range attack area")]  public float attack_range = 0.8f; 

    // Shooting needs
    [Header("Shooting Settings")]
    public GameObject bullet;
    [Tooltip("Timer before enemy can shoot again")] public float shoot_cooldown = 2.0f;
    [Tooltip("Where the projectile launches from")] public Transform shooting_hand;
    Vector3 shooting_direction; // Where the projectile is aimed
    Quaternion shooting_angle = new Quaternion();
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
    [Tooltip("How long the enemy is stunned")] public float stun_duration = 2.0f;
    [Tooltip("Rate of stun recovery")] public float stun_recovery = 2.0f;
    [Tooltip("How long enemy stays dizzy")] public float dizzy_duration;
    bool is_dizzy = false;
    float dizzy_time;

    bool is_stunned;
    float stun_time;

    // Death trigger
    bool is_dead = false;
    Collider collider;

    // Navigation rays
    struct PathRays
    {
        public Ray ray;
        public bool path_open;
    }

    [Header("Pathway decision")]
    PathRays[] nav_rays = new PathRays[8]; // The amount we're using in 8-directions
    [Tooltip("THe cooldown before enemy decides where to go")] public float path_choice_cooldown = 3.0f;
    [Tooltip("How much force the enemy jumps with")] public float jump_force = 10.0f;
    float nav_cooldown;

    [Header("Ground Check")]
    public float ground_ray_length = 3.0f;
    bool is_grounded;

    /* For the Sounds */
    SoundManager sound;

    /* Particles to be used and tranforms*/
    ParticleManager particles;
    ParticleSystem particle_effect;
    Transform particle_transform;

    // Sword Trails Particles : Dedicated melee particles
    public ParticleSystem melee_particle;

    // Things that need to be loaded before first frame
    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();

        // Search for sound
        sound = FindObjectOfType<SoundManager>();
        particles = FindObjectOfType<ParticleManager>();

        // From the forward ray all the way around
        //                6
        //             5  ^  7
        //              \ | /
        //         4  <- ( ) ->  0
        //              / | \
        //             3  v  1
        //                2
        for (int i = 0; i < nav_rays.Length; i++)
        {
            // From a starting point of 45 degrees
            float angle = 0.785398f * i;

            if (i == 0)
            {
                nav_rays[i].ray = new Ray(ray_centre.position, transform.forward);
            }
            else 
            {
                nav_rays[i].ray = new Ray(ray_centre.position, new Vector3(Mathf.Cos(angle), -Mathf.Sin(angle), 0)); 
            }
            nav_rays[i].path_open = false; // By default
        }

        // Dizzy
        dizzy_time = dizzy_duration;

        // Cooldown timer for Path navigation
        nav_cooldown = path_choice_cooldown;

        // On ground
        is_grounded = true;

        Random.InitState(System.DateTime.UtcNow.Second);
        
        // Get the player as target
        target = GameObject.FindGameObjectWithTag("player").transform;
        rb = GetComponent<Rigidbody>();

        // Rigidbody and rays
        ray = new Ray();

        if (ray_centre != null)
        {
            ray.origin = ray_centre.position;
        }

        // Check for particles
        if (particles != null)
        {
            switch (gameObject.tag)
            {
                case "skeleton":
                    particle_effect = particles.addParticle("SkeletonHitEffect", ray.origin, transform.rotation);
                    particle_transform = particle_effect.gameObject.transform;
                    break;
                case "spider":
                    particle_effect = particles.addParticle("SpiderBloodEffect", ray.origin, transform.rotation);
                    particle_transform = particle_effect.gameObject.transform;
                    break;
                default:
                    break;
            }
        }

       
        // They cast down no matter what
        ledge_ray = new Ray();
        ledge_ray.direction = -transform.up;

        Transform healthBarCanvas = gameObject.transform.Find("healthBarCanvas");
        healthBar = healthBarCanvas.gameObject.transform.Find("healthBar");

        max_health = health;

        healthSlider = healthBar.GetComponent<Slider>();
        healthSlider.maxValue = max_health;
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
        // Text Counter
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

        // groundCheck(ray, ground_ray_length);
     
        ray.direction = transform.forward;
        Debug.DrawRay(ray.origin, ray.direction, Color.green);

        // Set the Nav Rays' origins each frame.
        for (int i = 0; i < nav_rays.Length; i++)
        {
            nav_rays[i].ray.origin = ray_centre.position;
            Debug.DrawRay(nav_rays[i].ray.origin, nav_rays[i].ray.direction, Color.yellow);
        }
        
        if (!is_dead)
        {
            // As long as the enemy isn't stunned, do it's thing.
            if (!is_stunned && !is_dizzy)
            {
                  // Has detected player but is not within attack range
                  if (detectionZone())
                  {
                      if (Physics.Raycast(ray.origin, target.position - transform.position, out hit, detection_range, attack_layer.value))
                      {
                          // Is the player isn't in melee range
                          if (!attackRange())
                          {
                              if (can_shoot)
                              { 
                                  behaviour = STATE.SHOOT;
                              }
                              else
                              {
                                  behaviour = STATE.CHASING;
                              }
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

                  // Countdown the random timer
                  if (nav_cooldown > 0.0f)
                  {
                     nav_cooldown -= 1 * Time.deltaTime;
                  }

                  // Continuously look for an open path
                  pathCheck(nav_rays);

                  //// Check state to determine actions
                  switch (behaviour)
                  {
                      case 0: // Walking or patroling
                          walkForNothing();
                          break;
                      case (STATE)1: // Chasing
                          moveToPlayer();
                          break;
                      default:
                          break;
                  }

                  // For offensive actions
                  switch (behaviour)
                  {
                      case (STATE)2: // Attack
                        attack();
                          break;
                    case (STATE)3: // Shoot
                        if (shooting_hand != null)
                        {
                            shooting_direction = ((target.position + new Vector3(0, 1.2f, 0)) - shooting_hand.position).normalized; // Get the direction

                            if (transform.rotation.y > 0)
                            {
                                shooting_direction.z *= -1;
                            }

                            animator.SetFloat("x", shooting_direction.z);
                            animator.SetFloat("y", shooting_direction.y);
                        }
                        break;
                    default:
                        break;
                }

                  // Check in front of itself for obstacles or player
                  if (Time.timeScale > 0.0f)
                  { 
                    lineOfSight(ray);
                
                  }

                  // If the enemy has already attacked
                  if (!can_shoot)
                  {
                      shoot_timer -= Time.deltaTime;
                      if (shoot_timer <= 0.0f)
                      {
                          shoot_timer = shoot_cooldown;
                          can_shoot = true;
                      }
                  }
            }
            else if (is_dizzy)
            {
                hookStun();
            }
        }
    }



    /// <summary>
    /// Used with the animator. Play's grappling hook stun
    /// </summary>
    void hookStun()
    {
        dizzy_time -= Time.deltaTime;
            
        if (dizzy_time <= 0)
        {
            is_dizzy = false;
            dizzy_time = dizzy_duration;
        }
    }


    /// <summary>
    /// Play shooting animation the fire
    /// </summary>
    void shoot()
    {
        shooting_angle.SetFromToRotation(transform.forward, shooting_direction);

        if (shooting_hand != null)
            Instantiate(bullet, shooting_hand.position, shooting_angle);
        else
            Instantiate(bullet, transform.position + transform.forward, shooting_angle);

        // Check type of enemy for the sound to play
        switch (gameObject.tag)
        {
            case "skeleton":
                sound.playSound("fireballThrow");
                break;
            case "spider":
                sound.playSound("spiderwebThrow");
                break;
            default:
                break;
        }

        can_shoot = false;
    }

    // Shooting sound
    void shotReady()
    {
        sound.playSound("fireballThrow");
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
            Collider[] hits = Physics.OverlapSphere(hit_box.position, hit_range, attack_layer.value);

            foreach (Collider hit in hits)
            {
                GameObject player = hit.gameObject;
                Rigidbody player_rb = player.GetComponent<Rigidbody>();

                sound.playSound("playerDamaged_1");

                player.GetComponent<playerController1>().takeDamage(1.0f);
                player.GetComponent<playerController1>().knockBack(gameObject);
            }
            setAttack();
        }
    }

    // Allow animator to disable attack
    void setAttack()
    {
           is_attacking = false;
    }

    // Activate the attack sphere
    void activateAttack()
    {
        is_attacking = true;
    }  
    
    // Attack Sound
    void attackSound()
    {
        switch(gameObject.tag)
        {
            case "skeleton":
                sound.playSound("SkeletonMeleeAttack");
                break;
            case "spider":
                sound.playSound("spiderMelee attack");
                break;
            default:
                break;
        }
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

        if (distance_to_player < detection_range)
        {
            transform.forward = new Vector3((target.position - transform.position).x, 0, 0).normalized;
            return true;
        }
        return false;
    }

    void groundCheck(Ray ground_ray, float ray_distance)
    {
        // Look for the ground
        if (Physics.Raycast(ground_ray.origin, -transform.up, ray_distance, 9))
        {
            is_grounded = true;
        }
        else
        {
            is_grounded = false;
        }
        animator.SetBool("grounded", is_grounded);
    }

    void pathCheck(PathRays[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            if (Physics.Raycast(paths[i].ray, out hit, detection_range))
            {
                    paths[i].path_open = false;
            }
            else
            {
                // If this path is open, add it to the open path's array.
                paths[i].path_open = true;
            }
        }
    }

    /// <summary>
    /// If there is any obstacle in front, jump over it
    /// </summary>
    void jump()
    {
        int random_choice = Random.Range(0, nav_rays.Length);

        if (nav_rays[random_choice].path_open)
        {
            // If the direction of the ray points up in anyway.
            if (Vector3.Dot(nav_rays[random_choice].ray.direction, transform.up) > 0.0f)
            {
                rb.AddForce(nav_rays[random_choice].ray.direction * jump_force, ForceMode.VelocityChange);
            }
            else if (nav_rays[random_choice].ray.direction == new Vector3(0.7f, -0.7f, 0.0f)) // If it's slightly facing forward down
            {
                rb.AddForce(nav_rays[random_choice].ray.direction * (jump_force * 0.5f), ForceMode.VelocityChange);
            }
            else if (nav_rays[random_choice].ray.direction == new Vector3(-0.7f, -0.7f, 0.0f)) // If it's slightly facing backwards down
            {
                rb.AddForce(nav_rays[random_choice].ray.direction * (jump_force * 0.5f), ForceMode.VelocityChange);
            }
            nav_cooldown = path_choice_cooldown;
        }
    }

    /// <summary>
    /// Move towards the player
    /// </summary>
    void moveToPlayer()
    {
        Vector3 move_velocity = (target.position - transform.position).normalized;
        move_velocity = new Vector3(move_velocity.x, 0.0f, 0.0f);

        // Wherever  the player is, move towards them on the x-axis
        if (rb.velocity.magnitude < max_speed)
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

         if (!Physics.Raycast(ledge_ray, out hit, drop_cast_dist))
         {
             transform.forward *= -1;
         }

         if (rb.velocity.magnitude < max_speed)
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

    /// <summary>
    /// Disables this script, collider and plays death sound on death.
    /// </summary>
    void die()
    {
        // Check type of enemy for the sound to play
        switch (gameObject.tag)
        {
            case "skeleton":
                sound.playSound("skeletonDeath");
                break;
            case "spider":
                sound.playSound("spiderDeath");
                break;
            default:
                break;
        }

        // Reset Enemy velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        textCounter.subtract();
        gameObject.tag = "Untagged";
        gameObject.layer = 4;
        healthBar.gameObject.SetActive(false);
        rb.useGravity = false;
        collider.enabled = false;
        this.enabled = false;
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

    /// <summary>
    /// The dizzy bool trigger when using a trigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        switch (gameObject.tag)
        {
            case "skeleton":
                sound.playSound("skeletonDamaged_1");
                break;
            case "spider":
                sound.playSound("spiderDamaged_1");
                break;
        }

        if (other.gameObject.layer == 10)
        {        
            is_dizzy = true;
            // Reset Enemy velocity
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }


    /// <summary>
    /// The dizzy trigger using the collider
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 10)
        {
            switch (gameObject.tag)
            {
                case "skeleton":
                    sound.playSound("skeletonDamaged_1");
                    break;
                case "spider":
                    sound.playSound("spiderDamaged_1");
                    break;
                default:
                    break;
            }
            is_dizzy = true;
        }
    }

    // Damage function with knockback.
    public void enemyHealthDown(int damage)
    {
        health -= damage;
        healthSlider.value = health;

        // While still alive
        if (health > 0)
        { 
            is_stunned = true;

            // Check for sounds
            if (sound != null)
            {
                switch (gameObject.tag)
                {
                    case "skeleton":
                        sound.playSound("skeletonDamaged_1");
                        break;
                    case "spider":
                        sound.playSound("spiderDamaged_1");
                        break;
                }
            }

            // Reset Enemy velocity
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce((transform.up * knockback_vertical) + (-transform.forward * knockback_horizontal), ForceMode.VelocityChange);
        }

        if (particle_effect != null)
        {
            particle_transform.position = ray_centre.position;
            particle_transform.rotation = transform.rotation;
            particle_effect.Play();
        }


        // If it has no health points
        if (health <= 0.0f && !is_dead)
        {
            is_dead = true;
            animator.SetBool("Walk", false);
            animator.SetFloat("Stun Time", 0);
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Shoot");
            animator.ResetTrigger("Knockback");
            animator.SetTrigger("Death");
        }
    }

 
    /// <summary>
    /// Footstep sounds to be triggered via aniamtion events
    /// </summary>
    public void footstep()
    {
        sound.playSound("footstep_1");
    }

    /// <summary>
    /// Plays the melee trail during attack swing
    /// </summary>
    void meleeTrail()
    {
        melee_particle.Play();
    }

    /// <summary>
    /// Stops the melee trails
    /// </summary>
    void stopTrail()
    {
        melee_particle.Stop();
    }


    public bool IsDead
    {
        get { return is_dead; }
    }

    public int Health
    {
        get { return health; }
        set { health = value; }
    }

    void resetStun()
    {
        is_stunned = false;
    }

    public STATE State
    {
        get { return behaviour; }
    }

    public bool IsDizzy
    {
        get { return is_dizzy; }
    }

    public float StunTime
    {
        get { return dizzy_time; }
    }

    // Return the shoot available
    public bool canShoot
    {
        get { return can_shoot; }
        set { can_shoot = value; }
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
        set { is_stunned = value; }
    }

    public enum STATE
    {
        WALKING, // 0
        CHASING, // 1
        ATTACK,   // 2
        SHOOT   // 3
    }
}
