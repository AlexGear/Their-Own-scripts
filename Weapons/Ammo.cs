using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Ammo : PoolObject {
    [SerializeField] float hitRadius = 0.5f;
    [SerializeField] protected float speed = 0f;
    [SerializeField] protected float damage = 60f;
    [SerializeField] protected LayerMask hitLayerMask;
    [SerializeField] protected LayerMask damageLayerMask;
    protected bool hasHit;

    private Vector2 prevPosition;
    private bool checkedMeleeHit;

    public Unit shooter { get; set; }

    public const int maxRicochetCount = 6;
    [HideInInspector] public int ricochetCount;

    public float GetSpeed() => speed;

    public override void OnTaken() {
        base.OnTaken();

        shooter = null;
        hasHit = false;
        ricochetCount = 0;
        prevPosition = transform.position;
        checkedMeleeHit = false;
    }

    protected virtual void CheckMeleeHit() {
        if(shooter == null)
            return;

        Vector2 shooterPosition = shooter.position;
        Vector2 direction = (Vector2)transform.position - shooterPosition;
        
        RaycastHit2D raycast = Raycast(shooterPosition, direction, direction.magnitude, hitLayerMask);
        if (raycast.collider != null) {
            if(raycast.transform.CompareTag("Shield") && Shield.IsPointInside(shooterPosition)) {
                return;
            }
            transform.position = raycast.point;
            OnHit(raycast.transform, raycast.point, raycast.normal);
            Release(4f);
        }
    }

    protected virtual bool CanDamage(Unit unit) {
        return (damageLayerMask & (1 << unit.gameObject.layer)) != 0;
    }

    protected RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask) {
        return Physics2D.CircleCast(origin, hitRadius, direction, distance, layerMask);
    }

    protected RaycastHit2D RaycastShieldsSpecific(Vector2 start, Vector2 direction, float distance, int layerMask) {
        direction.Normalize();
        start -= direction * (hitRadius + 0.07f);
        distance += (hitRadius + 0.07f);
        while(true) {
            RaycastHit2D hit = Raycast(start, direction, distance, layerMask);
            if(hit.collider == null) {
                return hit;
            }
            if(hit.transform.CompareTag("Shield")) {
                Vector2 tinyNormal = hit.normal * 0.1f;
                if(Shield.IsPointInside(hit.point + tinyNormal)) {
                    start = hit.point - tinyNormal;
                    distance -= hit.distance;
                    continue;
                }
            }
            return hit;
        }
    }

    public virtual void Update() {
        if(!checkedMeleeHit) {
            CheckMeleeHit();
            checkedMeleeHit = true;
            return;
        }

        if(hasHit)
            return;

        RaycastHit2D hit;
        if(Shield.IsPointInside(transform.position) ||
           Shield.IsPointInside(transform.position + transform.up * hitRadius)) // sometimes the circlecast start point (transform.position) is outside of the shield but the actual circle at the start does overlap it, and thus the shield gets ignored and the bullet can fly through the shield
        {
            var direction = (Vector2)transform.position - prevPosition;
            hit = RaycastShieldsSpecific(prevPosition, direction, direction.magnitude, hitLayerMask);
            if(hit.collider != null) {
                OnHit(hit.transform, hit.point, hit.normal);
                return;
            }
        }

        hit = RaycastShieldsSpecific(transform.position, transform.up, speed * Time.deltaTime, hitLayerMask);
        if(hit.collider == null) {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            return;
        }

        prevPosition = transform.position;
        transform.position = hit.point;
        OnHit(hit.transform, hit.point, hit.normal);
    }

    protected virtual void OnHit(Transform hitTransform, Vector2 point, Vector2 normal) {
        hasHit = true;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        var color = Color.blue;
        color.a = 0.2f;
        Handles.color = color;

        Handles.DrawSolidDisc(transform.position, Vector3.forward, hitRadius);
    }
#endif
}