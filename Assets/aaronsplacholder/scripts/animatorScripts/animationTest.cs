using UnityEngine;

public class animationTest : MonoBehaviour
{
    public GameObject attackPoint;
    public float attackRange;
    public LayerMask enemyLayers;


    public void swordOn()
    {     

    }

    public void swordOff()
    {
    }
    public void playSwordSwingSound()
    {
        FindObjectOfType<SoundManager>().playSound("swordSwing");
    }

    public void createHitBox()
    {
        Collider[] enemiesHit = Physics.OverlapSphere(attackPoint.transform.position, attackRange, enemyLayers);
        
        foreach(Collider enemy in enemiesHit)
        {
            Debug.Log("Hit: " + enemy.transform.name);
            EnemyBehaviour behaviour = enemy.GetComponent<EnemyBehaviour>();
            behaviour.enemyHealthDown(1);
        }
    }
}
