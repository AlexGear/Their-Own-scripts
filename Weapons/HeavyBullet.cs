using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyBullet : Ammo {
    [SerializeField] private float trailDuration = 0.05f;
    [SerializeField] private GameObject bulletHit;
    [SerializeField] private GameObject bulletLight;
    private LineRenderer lineRenderer;
    private Vector3 trailEnd;
    private float timerTrail;
    [SerializeField] private GameObject repulsedBullet;

    private float remainingDamage;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void OnTaken() {
        base.OnTaken();
        timerTrail = 0;
        remainingDamage = damage;
        bulletHit.SetActive(false);
        bulletLight.SetActive(true);
        trailEnd = transform.position;
        lineRenderer.SetPosition(0, trailEnd);
        lineRenderer.SetPosition(1, trailEnd);

        Release(3);
    }

    public override void Update() {
        base.Update();
        lineRenderer.SetPosition(0, transform.position);
        if (timerTrail > trailDuration) {
            trailEnd = Vector3.MoveTowards(trailEnd, transform.position, speed * Time.deltaTime);
            lineRenderer.SetPosition(1, trailEnd);
        }
        else {
            timerTrail += Time.deltaTime;
        }
    }

    protected override void OnHit(Transform hitTransform, Vector2 point, Vector2 normal) {
        var unit = hitTransform.GetComponentInParent<Unit>();
        if (unit != null) {
            if(CanDamage(unit)) {
                float health = unit.health;
                unit.ApplyDamage(remainingDamage, shooter);
                remainingDamage -= health;
                if (remainingDamage > 0)
                {
                    return;
                }
                var blood = hitTransform.GetComponentInParent<Blood>();
                blood?.AddNewBloodStain(point, normal);
            }
        } else if (hitTransform.CompareTag("Shield")) {
            ShowBulletHit(point, normal);
            Shield.RepulseBullet(repulsedBullet, point, normal, ricochetCount);
        }
        else {
            ShowBulletHit(point, normal);
        }
        bulletLight.SetActive(false);
        Release(0.5f);
        base.OnHit(hitTransform, point, normal);
    }

    protected override void CheckMeleeHit() {
        base.CheckMeleeHit();
        lineRenderer.SetPosition(0, transform.position);
    }

    private void ShowBulletHit(Vector2 point, Vector2 normal) {
        bulletHit.transform.SetPositionAndRotation(point, Quaternion.LookRotation(Vector3.forward, normal));
        bulletHit.SetActive(true);
    }
}
