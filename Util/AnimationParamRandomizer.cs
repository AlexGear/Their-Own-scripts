using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationParamRandomizer : StateMachineBehaviour {
    [SerializeField] string paramName;
    [SerializeField] int min;
    [SerializeField] int max;

    private int hash;

    private void Awake() {
        hash = Animator.StringToHash(paramName);
    }
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        int value = Random.Range(min, max + 1);
        animator.SetInteger(hash, value);
	}
}
