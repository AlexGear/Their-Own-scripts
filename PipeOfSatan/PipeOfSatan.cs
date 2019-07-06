using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bezier;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NaughtyAttributes;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PipeOfSatan : MonoBehaviour, ISavable {
    [SerializeField, Required] Transform subCarrierPosition;
    [SerializeField] float speed = 9;
    [SerializeField] BezierPath headCarrierPath;
    [SerializeField] BezierPath subCarrierPath;
    [SerializeField] float minionCallRadius = 8;
    [SerializeField] LayerMask minionLayerMask;
    [SerializeField, Required] Collider2D headTrigger;
    [SerializeField, Required] Collider2D subTrigger;
    [Header("FX")]
    [SerializeField] ParticleSystem firingParticles;
    [SerializeField] ParticleSystem firingBeamLightParticles;

    private Collider2D trigger;
    private Animator animator;

    private Chassis chassis;
    private float length;
    private float currentVelocity = 0;
    // Smooth Damp Velocity
    private float currentVelocitySDV;

    private MinionPiper headCarrier;
    private MinionPiper subCarrier;

    private MinionPiper headCarrierRunning;
    private MinionPiper subCarrierRunning;

    private Timer callingCarriersTimer;
    private Timer joiningCheckTimer;

    private List<Collider2D> touchingStoppers = new List<Collider2D>();

    private bool isAttacking;
    private float attackFinishedTimestamp;

    public static PipeOfSatan current { get; private set; }

    private bool canMove => headCarrier.Is() && subCarrier.Is() && 
        touchingStoppers.Count == 0 && !isAttacking;


    public object CaptureSnapshot() {
        return chassis.CaptureSnapshot();
    }

    public void RestoreSnapshot(object data) {
        chassis.RestoreSnapshot(data);
        Align();
    }
    

    private void Awake() {
        current = this;
        trigger = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        length = Vector2.Distance(transform.position, subCarrierPosition.position);
        chassis = new Chassis(headCarrierPath, subCarrierPath, length);
        Align();

        callingCarriersTimer = new Timer(1f);
        joiningCheckTimer = new Timer(0.5f);
        CallCarriers();
    }

    private void CallCarriers() {
        if(headCarrierRunning.Is() && subCarrierRunning.Is()) {
            UpdateRunningDestination();
            return;
        }

        var minions = Unit.GetInRadius<MinionPiper>(transform.position, minionCallRadius, minionLayerMask)
            .Where(m => m != headCarrierRunning && m != subCarrierRunning);
        if(!headCarrierRunning.Is()) {
            headCarrierRunning = minions.ClosestTo(headTrigger.transform.position);
        }
        if(!subCarrierRunning.Is()) {
            subCarrierRunning = minions.ClosestTo(subTrigger.transform.position);
        }
        UpdateRunningDestination();
    }

    private void UpdateRunningDestination() {
        if(headCarrierRunning.Is()) {
            headCarrierRunning.RunToPosition(headTrigger.transform.position);
        }
        if(subCarrierRunning.Is()) {
            subCarrierRunning?.RunToPosition(subTrigger.transform.position);
        }
    }

    private void Update() {
        // If someone just died
        if(headCarrier != null && !headCarrier.Is() ||
            subCarrier != null && !subCarrier.Is()) 
        {
            DropPipe();
            DisbandCarriers();
            CallCarriers();
        }

        if(!headCarrier.Is() && !subCarrier.Is()) {
            if(!headCarrierRunning.Is() || !subCarrierRunning.Is()) {
                if(callingCarriersTimer.Tick()) {
                    CallCarriers();
                }
            }
            else if(joiningCheckTimer.Tick() && CanJoinRunningCarriers()) {
                // TODO: pipe loading animation
                JoinRunningCarriers();
            }
            else if(transform.hasChanged) {
                UpdateRunningDestination();
                transform.hasChanged = false;
            }
        }

        touchingStoppers.RemoveAll(c => c == null || !c.IsTouching(trigger));

        float targetVelocity = canMove ? speed : 0;
        const float smoothTime = 0.4f;
        currentVelocity = Mathf.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocitySDV, smoothTime);
        if(Mathf.Abs(currentVelocity) > 0.1f) {
            chassis.MoveWithVelocity(currentVelocity);
            Align();
        }
    }

    private void Align() {
        transform.position = chassis.frontPosition;

        const float smoothingAfterAttackingDuration = 3f;
        float timePassed = Time.time - attackFinishedTimestamp;
        if(timePassed > smoothingAfterAttackingDuration) {
            transform.rotation = chassis.rotation;
            return;
        }
        float t = timePassed / smoothingAfterAttackingDuration;
        transform.rotation = Quaternion.Slerp(transform.rotation, chassis.rotation, t);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("PipeOfSatanStoppers")) {
            touchingStoppers.Add(collision);
        }
        else if(collision.CompareTag("PipeOfSatanAttackTrigger")) {
            var attackTrigger = collision.GetComponent<PipeOfSatanAttackTrigger>();
            StartCoroutine(AttackCoroutine(attackTrigger));
            attackTrigger.gameObject.SetActive(false);
        }
    }

    private IEnumerator AttackCoroutine(PipeOfSatanAttackTrigger attackTrigger) {
        isAttacking = true;

        GameObject attackTarget = attackTrigger.GetTarget();
        // Delay before deploying
        yield return new WaitForSeconds(1f);

        float oldAngle = transform.rotation.eulerAngles.z;
        var toTarget = attackTarget.transform.position - transform.position;
        float destAngle = Quaternion.LookRotation(Vector3.forward, toTarget).eulerAngles.z;

        float angle = oldAngle;
        float smoothDampVel = 0;
        const float degsPerSecond = 45;
        float smoothTime = Mathf.Abs(Mathf.DeltaAngle(oldAngle, destAngle)) / degsPerSecond;

        // Turning towards the target
        while(Mathf.Abs(Mathf.DeltaAngle(angle, destAngle)) > 2f) {
            yield return null;
            if(headCarrier.Is() && subCarrier.Is()) {
                angle = Mathf.SmoothDampAngle(angle, destAngle, ref smoothDampVel, smoothTime);
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            else {
                smoothDampVel = 0;
            }
        }
        
        // Charging up the pipe
        while(true) {
            yield return new WaitUntil(() => headCarrier.Is() && subCarrier.Is());

            animator.SetBool("ChargingUp", true);
            const float chargingDuration = 2.9f;
            var timer = new Timer(chargingDuration);
            yield return new WaitUntil(() => timer.Tick() || !headCarrier.Is() || !subCarrier.Is());

            animator.SetBool("ChargingUp", false);
            if(headCarrier.Is() && subCarrier.Is()) {
                // Fire
                break;
            }
        }

        float headMaxHP = headCarrier.maxHealth, headHP = headCarrier.health;
        float subMaxHP = subCarrier.maxHealth, subHP = subCarrier.health;
        // Preventing minions from being killed while firing
        headCarrier.maxHealth = subCarrier.maxHealth = 99999;
        headCarrier.health = subCarrier.health = 99999;

        // Firing
        if(firingParticles != null || firingBeamLightParticles != null) {
            float distance = toTarget.magnitude;
            const float secsPerUnit = 0.004f;

            if(firingParticles != null) {
                var main = firingParticles.main;
                main.startLifetimeMultiplier = distance * secsPerUnit;
            }
            if(firingBeamLightParticles != null) {
                var main = firingBeamLightParticles.main;
                main.startLifetimeMultiplier = distance * secsPerUnit;
            }
        }
        animator.SetTrigger("Fire");
        yield return new WaitForSeconds(0.05f);

        // Evaporation
        const float evaporationDuration = 7.5f;
        attackTarget.GetComponent<Animator>()?.SetTrigger("Evaporate");
        yield return new WaitForSeconds(evaporationDuration);

        headCarrier.health = headHP;
        headCarrier.maxHealth = headMaxHP;
        subCarrier.health = subHP;
        subCarrier.maxHealth = subMaxHP;

        attackTarget.SetActive(false);

        attackFinishedTimestamp = Time.time;
        isAttacking = false;
    }

    private void DropPipe() {
        // TODO: drop animation
    }

    private void DisbandCarriers() {
        if(headCarrier != null) {
            headCarrier.DisjoinPipeOfSatan();
            headCarrier = null;
        }
        if(subCarrier != null) {
            subCarrier.DisjoinPipeOfSatan();
            subCarrier = null;
        }
    }

    private bool CanJoinRunningCarriers() {
        return headTrigger.IsTouching(headCarrierRunning.collider) &&
                subTrigger.IsTouching(subCarrierRunning.collider);
    }

    private void JoinRunningCarriers() {
        headCarrier = headCarrierRunning;
        subCarrier = subCarrierRunning;
        headCarrierRunning = null;
        subCarrierRunning = null;

        headCarrier.JoinPipeOfSatan(headTrigger.transform);
        subCarrier.JoinPipeOfSatan(subTrigger.transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, Vector3.forward, minionCallRadius);
    }
#endif
}
