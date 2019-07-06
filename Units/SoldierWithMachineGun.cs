using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierWithMachineGun : MonoBehaviour {
    [SerializeField] CommonWeapon machineGun;
    [SerializeField] float clampAngle = 60;
    [SerializeField] float rotationSpeed = 60;
    [SerializeField] LayerMask targetLayers;
    [SerializeField] LayerMask visionObstacles;
    [SerializeField] Collider2D attackArea;
    [SerializeField] Animator fireAnimator;

    private const float targetUpdateInterval = 0.3f;
    private static readonly int fireHash = Animator.StringToHash("Fire");

    private static Collider2D[] overlapResults = new Collider2D[40];

    private ContactFilter2D filter;
    private float halfClampAngleCos;
    private Transform gunTransform;
    private Unit target = null;

    private Vector2 initialDirection => transform.up;

    private void Awake() {
        gunTransform = machineGun.transform;
        filter = new ContactFilter2D { layerMask = targetLayers, useLayerMask = true };
        halfClampAngleCos = Mathf.Cos(Mathf.Deg2Rad * clampAngle / 2);
        if(fireAnimator != null) {
            machineGun.OnFire += _ => fireAnimator.SetTrigger(fireHash);
        }

        StartCoroutine(TargetUpdateCoroutine());
    }

    private void Update() {
        Quaternion destination;
        if(target != null) {
            Vector2 direction;

            var movingUnit = target as MovingUnit;
            if(movingUnit == null)
                direction = target.position - (Vector2)gunTransform.position;
            else
                direction = Deflection.CalculateDirection(gunTransform.position, target.position, machineGun.bulletSpeed, movingUnit.velocityTrend);

            destination = GetClampedLookRotation(direction);
        }
        else
            destination = Quaternion.LookRotation(Vector3.forward, initialDirection);

        gunTransform.rotation = Quaternion.RotateTowards(gunTransform.rotation, destination, rotationSpeed * Time.deltaTime);

        machineGun.isTriggerPressed = target != null;
    }

    private void OnDisable() {
        machineGun.isTriggerPressed = false;
    }

    private Quaternion GetClampedLookRotation(Vector2 direction) {
        float angle = Vector2.SignedAngle(initialDirection, direction);
        angle = Mathf.Clamp(angle, -clampAngle / 2, clampAngle / 2);
        Quaternion rotationToTarget = Quaternion.AngleAxis(angle, Vector3.forward);
        return Quaternion.LookRotation(Vector3.forward, initialDirection) * rotationToTarget;
    }

    private void OnTargetDied(Unit target) {
        target.Died -= OnTargetDied;
        UpdateTarget();
    }

    private void UpdateTarget() {
        target = GetClosestTarget();
        if(target != null) {
            target.Died += OnTargetDied;
        }
    }

    private Unit GetClosestTarget() {
        Vector2 gunPosition = gunTransform.position;

        int n = attackArea.OverlapCollider(filter, overlapResults);

        Unit nearestUnit = null;
        float minSqrDistance = float.PositiveInfinity;
        for(int i = 0; i < n; i++) {
            Unit unit = overlapResults[i].GetComponentInParent<Unit>();
            if(unit == null || !IsVisible(unit) || !IsInFOV(unit))
                continue;

            float sqrDistance = (unit.position - gunPosition).sqrMagnitude;
            if(sqrDistance < minSqrDistance) {
                minSqrDistance = sqrDistance;
                nearestUnit = unit;
            }
        }

        return nearestUnit;
    }

    private bool IsVisible(Unit unit) {
        RaycastHit2D hit = Physics2D.Linecast(gunTransform.position, unit.position, visionObstacles);
        return hit.collider == null;
    }

    private bool IsInFOV(Unit unit) {
        Vector2 gunPosition = gunTransform.position;
        Vector2 toUnitDirection = (unit.position - gunPosition).normalized;
        return Vector2.Dot(initialDirection, toUnitDirection) > halfClampAngleCos;
    }

    private IEnumerator TargetUpdateCoroutine() {
        while(true) {
            yield return new WaitForSeconds(targetUpdateInterval);
            UpdateTarget();
        }
    }
    
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(clampAngle / 2, Vector3.forward) * transform.up * 40);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-clampAngle / 2, Vector3.forward) * transform.up * 40);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis( clampAngle / 2, Vector3.forward) * -transform.up * 20);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-clampAngle / 2, Vector3.forward) * -transform.up * 20);
    }
}
