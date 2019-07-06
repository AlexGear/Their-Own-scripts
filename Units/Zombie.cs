using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ZombieStats : UnitStats {
    public float attackRange;
    public float attackAreaRadius;
    public float attackDamage;
}

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class Zombie : AnimatedUnit {
    public ZombieStats stats;
    [SerializeField] private GameObject[] detachPartsOnDeath;
    [SerializeField] private float detachMaxForce = 10.0f;
    [SerializeField] private int detachedPartsOrder = 5;
    [SerializeField] private Material detachedPartsMaterial;
    
    private UnityEngine.AI.NavMeshAgent navAgent;
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int damageTakenHash = Animator.StringToHash("DamageTaken");

    private static Transform _detachedPartsRoot;
    private static Transform detachedPartsRoot => _detachedPartsRoot != null ? _detachedPartsRoot :
        (_detachedPartsRoot = new GameObject("Detached Parts").transform);

    protected override void Awake() {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        base.Awake();
    }

    protected override UnitAI CreateAI() => new ZombieAI(this, navAgent, stats);

    public void StartAttack() {
        animator.SetTrigger(attackHash);
    }

    public void AttackDamage() {
        Vector2 attackCenter = transform.position + transform.up * (stats.attackRange - stats.attackAreaRadius);
        foreach(var collider in Physics2D.OverlapCircleAll(attackCenter, stats.attackAreaRadius, stats.attackTargets)) {
            collider.GetComponentInParent<Unit>()?.ApplyDamage(stats.attackDamage, this);
        }
    }

    public void AttackFinished() {
        (ai as AI.ZombieAI).ContinueTargetPursuing();
    }

    public override void ApplyDamage(float value, Unit source) {
        base.ApplyDamage(value, source);
        if(isActiveAndEnabled) {
            animator.SetTrigger(damageTakenHash);
        }
    }

    protected override void OnDied() {
        if(!usedInPool)
            DetachParts();

        var blood = GetComponent<Blood>();
        if(blood != null) {
            blood.SpawnBloodBurst();
            blood.ClearBloodStains();
        }

        base.OnDied();
    }

    private void DetachParts() {
        foreach(var part in detachPartsOnDeath) {
            DetachPart(part);
        }
    }

    private void DetachPart(GameObject obj) {
        obj.transform.parent = detachedPartsRoot;
        obj.layer = Physics2D.IgnoreRaycastLayer;
        Collider2D collider = obj.GetComponent<Collider2D>();
        if(collider != null) {
            collider.enabled = true;
            Destroy(collider, 5f);
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if(rb != null) {
            rb.simulated = true;
            Vector2 force = Random.insideUnitCircle * detachMaxForce;
            rb.AddForceAtPosition(force, this.transform.position);
            Destroy(rb, 5f);
        }

        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach(var renderer in renderers) {
            renderer.sortingOrder = detachedPartsOrder;
            renderer.material = detachedPartsMaterial;
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.forward, stats.attackRange);

        Vector3 attackCenter = transform.position + transform.up * (stats.attackRange - stats.attackAreaRadius);
        Handles.color = Color.red;
        Handles.DrawWireDisc(attackCenter, Vector3.forward, stats.attackAreaRadius);

        Color cyan = Color.cyan;
        cyan.a = 0.15f;
        Handles.color = cyan;
        Vector3 from = Quaternion.Euler(0, 0, -stats.fovAngle * 0.5f) * transform.up;
        Handles.DrawSolidArc(transform.position, Vector3.forward, from, stats.fovAngle, stats.visionRange);
    }
#endif
}
