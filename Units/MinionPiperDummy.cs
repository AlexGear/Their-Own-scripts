using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MinionPiper))]
public class MinionPiperDummy : AnimatedUnit {
    private readonly int damageTakenHash = Animator.StringToHash("DamageTaken");
    public MinionPiper minion { get; private set; }

    protected override void Awake() {
        base.Awake();
        minion = GetComponent<MinionPiper>();
        maxHealth = minion.maxHealth;
    }

    private void OnEnable() {
        health = minion.health;
        animator.SetFloat(moveXHash, 0);
        animator.SetFloat(moveYHash, 1);
    }

    public override void ApplyDamage(float value, Unit source) {
        base.ApplyDamage(value, source);
        if(isActiveAndEnabled) {
            animator.SetTrigger(damageTakenHash);
        }
    }

    protected override void OnDied() {
        minion.health = health;
    }
}
