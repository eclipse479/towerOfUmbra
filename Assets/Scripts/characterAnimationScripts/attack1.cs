using UnityEngine;

public class attack1 : StateMachineBehaviour
{
    static playerController1 control;
    private void Awake()
    {
        control = GameObject.FindGameObjectWithTag("player").gameObject.GetComponent<playerController1>();
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("firstAttack", false);
        if(control.attackNumber > 3)
        {
            control.attackNumber = 3; // sets up the 2nd attack
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       if(control.attackNumber >= 2)
       {
            //control.attackNumber = 2; //  keeps it at 2
            animator.SetBool("secondAttack", true);
       }
    }
}
