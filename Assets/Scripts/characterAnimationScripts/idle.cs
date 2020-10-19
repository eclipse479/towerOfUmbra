using UnityEngine;

public class idle : StateMachineBehaviour
{
    static playerController1 control;
    private void Awake()
    {
        control = GameObject.FindGameObjectWithTag("player").gameObject.GetComponent<playerController1>();
    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        control.attackNumber = 0;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
