using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractiveObject {
    [SerializeField] private bool isOpen = false;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.SetBool("IsOpen", isOpen);
    }

    protected override void OnUsePressed() {
        isOpen = !isOpen;
        animator.SetBool("IsOpen", isOpen);
    }
}
