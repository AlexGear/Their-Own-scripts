using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class SoldierStats : UnitStats {
    public float fireDistance;
    public float minTargetDistance;
    public LayerMask bulletObstacles;
    public bool disallowHiding;
}

public class Soldier : AnimatedUnit {
    [SerializeField] SoldierStats stats;

    protected UnityEngine.AI.NavMeshAgent navAgent;
    protected BaseWeapon weapon;

    private readonly int shootHash = Animator.StringToHash("Shoot");
    private readonly int damageTakenHash = Animator.StringToHash("DamageTaken");
    private int weaponTypeHash = Animator.StringToHash("WeaponType");
    private int weaponSelectHash = Animator.StringToHash("WeaponSelect");

    protected override void Awake() {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        weapon = GetComponentInChildren<BaseWeapon>();
        weapon.OnFire += OnWeaponFire;

        base.Awake();
    }

    protected override UnitAI CreateAI() => new AI.SoldierAI(this, navAgent, stats, weapon);

    protected override void Start() {
        base.Start();
        WeaponSelectAnimation();
    }

    private void WeaponSelectAnimation() {
        animator.SetInteger(weaponTypeHash, weapon.animationType);
        animator.SetTrigger(weaponSelectHash);
    }

    private void OnWeaponFire(BaseWeapon weapon) {
        animator.SetTrigger(shootHash);
    }

    public override void ApplyDamage(float value, Unit source) {
        base.ApplyDamage(value, source);
        if(isActiveAndEnabled) {
            animator.SetTrigger(damageTakenHash);
        }
    }

    protected override void OnDied() {
        var blood = GetComponent<Blood>();
        if(blood != null) {
            blood.SpawnBloodBurst();
            blood.ClearBloodStains();
        }
        base.OnDied();
    }
    
#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.forward, stats.fireDistance);

        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, Vector3.forward, stats.minTargetDistance);

        Color cyan = Color.cyan;
        cyan.a = 0.04f;
        Handles.color = cyan;
        Vector3 from = Quaternion.Euler(0, 0, -stats.fovAngle * 0.5f) * transform.up;
        Handles.DrawSolidArc(transform.position, Vector3.forward, from, stats.fovAngle, stats.visionRange);
    }
#endif
}
