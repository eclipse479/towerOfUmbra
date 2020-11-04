using UnityEngine;

public class animationTest : MonoBehaviour
{
    public GameObject attackPoint;
    public float attackRange;
    public LayerMask enemyLayers;
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }

    public void swordOff()
    {
    }

    public void playSwordSwingSound()
    {
       soundManager.playSound("swordSwing");
    }

    public void playLandingSound()
    {
        soundManager.playSound("playerLand");
    }
    public void playFootstepSound()
    {
        soundManager.playSound("footstep_1");
    }

    public void attack1Particle()
    {
        ParticleManager.instance.addParticle("Swordslash", attackPoint.transform.position, Quaternion.Euler(0, 45, 0));
    }
    public void attack2Particle()
    {
        ParticleManager.instance.addParticle("Swordslash", attackPoint.transform.position, Quaternion.Euler(0, 0, 0));
    }
    public void attack3Particle()
    {
        ParticleManager.instance.addParticle("Swordslash", attackPoint.transform.position, Quaternion.Euler(90, 0, 0));
    }

    public void createHitBox()
    {
        Collider[] enemiesHit = Physics.OverlapSphere(attackPoint.transform.position, attackRange, enemyLayers);
        
        foreach(Collider enemy in enemiesHit)
        {
            EnemyBehaviour behaviour = enemy.GetComponent<EnemyBehaviour>();
            behaviour.enemyHealthDown(1);
        }
    }
}
