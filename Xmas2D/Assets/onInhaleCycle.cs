using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onInhaleCycle : StateMachineBehaviour {

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
//	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
//    }

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        int inhaleCharges = animator.GetInteger("inhaleCharges");
        inhaleCharges = inhaleCharges - 1;
        animator.SetInteger("inhaleCharges", inhaleCharges);
        if (inhaleCharges < 1)
        {
            animator.SetInteger("State", 7);
            animator.SetInteger("timeoutCount", 100);
            animator.SetBool("isInhaling", false);
        }
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int state = animator.GetInteger("State");

        if (state != 6 && state != 7 && state != 4)
        {
            animator.SetBool("canMove", true);
        }
        animator.SetInteger("inhaleCharges", 500);
        animator.SetBool("isInhaling", false);
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
