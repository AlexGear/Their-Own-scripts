using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Abilities/ShieldAbility")]
public class ShieldAbility : ContinuousAbility {
    [SerializeField] private GameObject prefab;
    [SerializeField] private float radius;
    private Shield shield;
    private GameObject compositeShield;
    private CompositeCollider2D shieldCollider;
    private Collider2D unitCollider;
    private NavMeshAgent unitNavAgent;

    protected override void Start() {
        base.Start();
        compositeShield = CompositeShield.current.gameObject;
        shieldCollider = compositeShield.GetComponent<CompositeCollider2D>();
        shield = Instantiate(prefab, transform.position, transform.rotation, compositeShield.transform).GetComponent<Shield>();
        shield.ownerTransform = transform;
        shield.radius = radius;
        shield.gameObject.SetActive(isActive);
        unitCollider = GetComponentInChildren<Collider2D>();
        unitNavAgent = GetComponent<NavMeshAgent>();
    }

    protected override void OnActivated() {
        Physics2D.IgnoreCollision(unitCollider, shieldCollider);
        shield.gameObject.SetActive(true);
        if(unitNavAgent != null) {
            unitNavAgent.SetShieldHolder(true);
        }
    }

    protected override void OnDeactivated() {
        shieldCollider.edgeRadius = 0;
        shield.gameObject.SetActive(false);
        if (Shield.IsPointInside(unit.transform.position)) {
            Vector2 exit = GetExitFromShield();
            StartCoroutine(MoveOwnerCoroutine(exit, 0.2f));
        }
        else {
            if(unitNavAgent != null) {
                unitNavAgent.SetShieldHolder(false);
            }
            Physics2D.IgnoreCollision(unitCollider, shieldCollider, false);
        }
    }

    private Vector2 GetExitFromShield() {
        Shield shield = Shield.OverlapShield(unit.transform.position);
        if(shield == null) {
            throw new System.Exception($"Unit {unit} was not inside a shield");
        }
        Vector2 closestPos = GetClosetPositionOutside(shield.transform.position);
        if (IsExitFromShield(closestPos)) {
            return closestPos;
        }
        int casts = 12;
        float angleStep = 360 / casts;
        float angle = 0;
        int sign = 1;
        Vector2 prospectivePosition = closestPos;
        Vector2 center = shield.transform.position;
        Vector2 direction = prospectivePosition - center;
        for (int i = 0; i < casts; i++) {
            if (IsExitFromShield(prospectivePosition)) {
                return prospectivePosition;
            }
            angle += angleStep;
            sign = -sign;
            direction = Quaternion.Euler(0, 0, sign * angle) * direction;
            prospectivePosition = center + direction;
        }
        return unit.transform.position;
    }

    private Vector2 GetClosetPositionOutside(Vector2 shieldCenter) {
        shieldCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
        ColliderDistance2D distance = unitCollider.Distance(shieldCollider);
        shieldCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        return distance.pointB + (distance.pointB - shieldCenter).normalized * 1.5f;
    }

    private bool IsExitFromShield(Vector2 point) {
        if(Shield.IsPointInside(point)) {
            return false;
        }
        if(Physics2D.OverlapPoint(point) != null) {
            return false;
        }

        Vector2 raycastOrigin = point;
        Vector2 raycastDirection = (Vector2)unit.transform.position - raycastOrigin;
        RaycastHit2D raycastHit = Physics2D.Raycast(raycastOrigin, raycastDirection);
        if(raycastHit.collider == unitCollider) {
            return true;
        }
        Vector2 tinyNormal = raycastHit.normal * 0.1f;
        if (!Shield.IsPointInside(raycastHit.point + tinyNormal) && raycastHit.transform.CompareTag("Shield")) {
            raycastOrigin = raycastHit.point - tinyNormal;
            shieldCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
            raycastHit = Physics2D.Raycast(raycastOrigin, raycastDirection);
            shieldCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            if (raycastHit.collider == unitCollider) {
                return true;
            }
        }
        return false;
    }

    IEnumerator MoveOwnerCoroutine(Vector2 moveTo, float time) {
        Vector2 startPos = unit.transform.position;
        for (float t = 0; t < time; t += Time.deltaTime) {
            unit.transform.position = Vector2.Lerp(startPos, moveTo, t / time);
            yield return null;
        }
        Physics2D.IgnoreCollision(unitCollider, shieldCollider, false);
        if(unitNavAgent != null) {
            unitNavAgent.SetShieldHolder(false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Color cyan = Color.cyan;
        cyan.a = 0.15f;
        Handles.color = cyan;
        Handles.DrawSolidDisc(transform.position, Vector3.forward, radius);
        if (Application.isPlaying && Shield.IsPointInside(unit.transform.position)) {
            Handles.color = Color.red;
            Handles.DrawLine(unit.transform.position, GetExitFromShield());
        }
    }

    [ContextMenu("Select attached shield")]
    private void SelectAttachedShield() {
        if (shield != null) {
            Selection.activeGameObject = shield.gameObject;
        }
    }
#endif
}
