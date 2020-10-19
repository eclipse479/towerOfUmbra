using UnityEngine;

public class attack2 : StateMachineBehaviour
{
    static playerController1 control;
    private bool attackAgain = false;

    private void Awake()
    {
        control = GameObject.FindGameObjectWithTag("player").gameObject.GetComponent<playerController1>();
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("secondAttack", false);
        if (control.attackNumber > 3)
        {
            control.attackNumber = 3; // sets up the 3rd attack
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (control.attackNumber >= 3)
        {
            //control.attackNumber = 3; // keeps it at 3
            animator.SetBool("thirdAttack", true);
        }
    }
}
