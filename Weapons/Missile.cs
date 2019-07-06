using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : Ammo {
    [SerializeField] Explosion explosion;
    [SerializeField] ParticleSystem jetTrail;
    [SerializeField] GameObject jetLight;
    private ParticleSystem.EmissionModule jetTrailEmitter;
    private CameraShake camShake;
    private SpriteRenderer spriteRenderer;
    private float startSpeed;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        jetTrailEmitter = jetTrail.emission;
        camShake = GetComponent<CameraShake>();
        startSpeed = speed;
    }

    public override void OnTaken() {
        base.OnTaken();
        speed = startSpeed;
        explosion.baseDamage = damage;
        explosion.targetsLayerMask = damageLayerMask;

        spriteRenderer.enabled = true;
        jetLight.SetActive(true);
        jetTrailEmitter.enabled = true;
        Release(10);
    }

    protected override void OnHit(Transform hitTransform, Vector2 point, Vector2 normal) {
        base.OnHit(hitTransform, point, normal);

        explosion.Explode(shooter);

        speed = 0f;
        jetLight.SetActive(false);
        jetTrailEmitter.enabled = false;
        spriteRenderer.enabled = false;
        if(camShake != null)
            camShake.ShakeCameraOmni();
        Release(5);
    }
}
