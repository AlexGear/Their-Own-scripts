using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAbility : TargetPointAbility {
    [SerializeField] float maxDistance = 10f;
    [SerializeField] float unitBodyRadius = 0.6f;
    [SerializeField] LayerMask hindrances;
    [SerializeField] LayerMask shieldsLayer;
    [SerializeField] ParticleSystem particleA;
    [SerializeField] ParticleSystem particleB;
    [SerializeField] string restrictedAreasTag;

    private int layerMask;
    private ShieldAbility shieldAbility;

    private static Collider2D[] overlapResults = new Collider2D[50];

    private void Awake() {
        layerMask = hindrances;

        shieldAbility = GetComponent<ShieldAbility>();
        if(shieldAbility != null) {
            shieldAbility.Activated += OnShieldActivated;
            shieldAbility.Deactivated += OnShieldDeactivated;
        }
    }

    private void OnDestroy() {
        if(shieldAbility != null) {
            shieldAbility.Activated -= OnShieldActivated;
            shieldAbility.Deactivated -= OnShieldDeactivated;
        }
    }

    private void OnShieldActivated(Ability ability) {
        layerMask = hindrances & ~shieldsLayer;
    }

    private void OnShieldDeactivated(Ability ability) {
        layerMask = hindrances;
    }

    protected override bool CanUse(Vector2 at) {
        if(((Vector2)transform.position - at).CompareLength(maxDistance) > 0) {
            return false;
        }
        return IsPlaceClear(at);
    }

    private bool IsPlaceClear(Vector2 point) {
        if(!string.IsNullOrEmpty(restrictedAreasTag)) {
            using(new QueriesHitTriggers(true)) {
                int n = Physics2D.OverlapPointNonAlloc(point, overlapResults, layerMask);
                for(int i = 0; i < n; i++) {
                    var trigger = overlapResults[i];
                    if(trigger.isTrigger && trigger.CompareTag(restrictedAreasTag)) {
                        return false;
                    }
                }
            }
        }
        var collider = Physics2D.OverlapCircle(point, unitBodyRadius, layerMask);
        return collider == null || collider == unit.collider;
    }

    protected override Vector2? TryFindAnotherUsePoint(Vector2 originalPoint) {
        Vector2 position = transform.position;
        Vector2 vector = originalPoint - position;
        Vector2 point = position + Vector2.ClampMagnitude(vector, maxDistance);
        if(IsPlaceClear(point)) {
            return point;
        }

        Vector2 result = position;
        Vector2 raycastOrigin = transform.position;
        while((raycastOrigin - point).sqrMagnitude > 0.05f) {
            RaycastHit2D hit;
            hit = Physics2D.Linecast(raycastOrigin, point, layerMask);

            if(hit.collider == null) {
                return result;
            }
            Vector2 shiftedHitPoint = hit.point + hit.normal * (unitBodyRadius + 0.05f);
            if(IsPlaceClear(shiftedHitPoint)) {
                result = shiftedHitPoint;
            }
            raycastOrigin = hit.point - hit.normal * 0.01f;
        }
        return result;
    }

    protected override void Use(Vector2 at) {
        base.Use(at);

        particleA.transform.position = at;
        particleB.transform.position = this.transform.position;
        particleA.Play();
        particleB.Play();

        transform.position = at;
    }
}
