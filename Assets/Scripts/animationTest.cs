using UnityEngine;

public class animationTest : MonoBehaviour
{
    public GameObject attackPoint;
    public float attackRange;
    public LayerMask enemyLayers;
    private SoundManager soundManager;
    public Transform swordTip;
    private playerController1 control;

    private ParticleManager manager;
    private ParticleSystem landing;
    private Transform landingTransform;
    private void Awake()
    {
        manager = FindObjectOfType<ParticleManager>();
        control = gameObject.transform.parent.GetComponent<playerController1>();
        soundManager = FindObjectOfType<SoundManager>();
        landing = manager.addParticle("Footsteps impact", transform.position, Quaternion.Euler(0,0,0));
        landingTransform = landing.gameObject.transform;
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
        landingTransform.position = gameObject.transform.position;
        landing.Play();
        soundManager.playSound("playerLand");
    }
    public void stopLandingParticle()
    {
        landing.Stop();
    }
    public void playJumpingSound()
    {
        soundManager.playSound("jumping");
    }
    public void playFootstepSound()
    {
        control.playFootstep();
    }

    public void attackParticle()
    {
        if(swordTip.gameObject.activeSelf)
        swordTip.gameObject.SetActive(false);
        else
        swordTip.gameObject.SetActive(true);
        
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
