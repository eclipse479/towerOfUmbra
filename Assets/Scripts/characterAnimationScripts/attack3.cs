using UnityEngine;

public class attack3 : StateMachineBehaviour
{
    static playerController1 control;
    private void Awake()
    {
        control = GameObject.FindGameObjectWithTag("player").gameObject.GetComponent<playerController1>();
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("thirdAttack", false);
        control.attackNumber = 0; // reset attack combo
    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("attacking", false);
        Debug.Log("stop attacking");
    }
}