using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class MinionPiper : AnimatedUnit {
   // public UnitStats stats;
    [SerializeField] private GameObject[] detachPartsOnDeath;
    [SerializeField] private float detachMaxForce = 10.0f;
    [SerializeField] private int detachedPartsOrder = 5;
    [SerializeField] private Material detachedPartsMaterial;

    public bool hasJoinedPipe { get; private set; }

    private UnityEngine.AI.NavMeshAgent navAgent;
    private readonly int damageTakenHash = Animator.StringToHash("DamageTaken");

    private static Transform _detachedPartsRoot;
    private static Transform detachedPartsRoot => _detachedPartsRoot != null ? _detachedPartsRoot :
        (_detachedPartsRoot = new GameObject("Detached Parts").transform);

    protected override void Awake() {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        base.Awake();
    }

    protected override void Start() {
        base.Start();
        ((MinionPiperAI)ai).FollowPipe();
    }

    public void RunToPosition(Vector2 position) {
        ai.state = new MoveToPositionState(ai, position);
    }

    public void JoinPipeOfSatan(Transform parent) {
        navAgent.enabled = false;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        
        ai.state = null;

        hasJoinedPipe = true;
    }

    public void DisjoinPipeOfSatan() {
        navAgent.enabled = true;
        transform.parent = null;

        hasJoinedPipe = false;
    }

    protected override UnitAI CreateAI() => new MinionPiperAI(this, navAgent);

    public override void ApplyDamage(float value, Unit source) {
        base.ApplyDamage(value, source);
        if(isActiveAndEnabled) {
            animator.SetTrigger(damageTakenHash);
        }
    }

    protected override void OnDied() {
        DisjoinPipeOfSatan();

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
/*
#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Color cyan = Color.cyan;
        cyan.a = 0.15f;
        Handles.color = cyan;
        Vector3 from = Quaternion.Euler(0, 0, -stats.fovAngle * 0.5f) * transform.up;
        Handles.DrawSolidArc(transform.position, Vector3.forward, from, stats.fovAngle, stats.visionRange);
    }
#endif*/
}
