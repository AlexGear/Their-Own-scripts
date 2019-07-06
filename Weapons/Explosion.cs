using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    public enum DamageFalloffType { NoFalloff, Linear, InverseSquare }

    public bool explodeOnAwake = false;
    public float radius = 1;
    public float baseDamage = 100;
    public DamageFalloffType damageFalloffType = DamageFalloffType.InverseSquare;
    public float damageDelay = 0.1f;
    public LayerMask targetsLayerMask;
    public LayerMask obstaclesLayerMask;
    public float knockbackForce = 0;

    private ParticleSystem particles;
    private CameraShake cameraShake;
    private float scaledRadius => radius * transform.lossyScale.x;

    private void Awake() {
        particles = GetComponent<ParticleSystem>();
        cameraShake = GetComponent<CameraShake>();
        if(explodeOnAwake) {
            Explode();
        }
    }

    public void Explode() {
        Explode(null);
    }

    public void Explode(Unit source) {
        StartCoroutine(ExplodeRoutine(source));
    }

    private IEnumerator ExplodeRoutine(Unit source = null) {
        if(particles != null)
            particles.Play();
        if(cameraShake != null)
            cameraShake.ShakeCameraOmni();

        yield return new WaitForSeconds(damageDelay);

        Vector2 center = transform.position;
        var targets = Unit.GetInRadius<Unit>(center, scaledRadius, targetsLayerMask, obstaclesLayerMask);
        foreach(Unit unit in targets) {
            float damage = CalculateDamage(unit.position);
            unit.ApplyDamage(damage, source);
            if(knockbackForce != 0) {
                var knockback = unit.GetComponentInParent<Knockback>();
                knockback?.FromEpicenter(center, knockbackForce, 0.2f);
            }
        }
    }

    private float CalculateDamage(Vector2 targetPosition) {
        if(damageFalloffType == DamageFalloffType.NoFalloff)
            return baseDamage;

        float distance = Vector2.Distance(transform.position, targetPosition);
        float distanceFactor = distance / scaledRadius;
        switch(damageFalloffType) {
            case DamageFalloffType.Linear:
                return baseDamage * Mathf.Clamp01(1 - distanceFactor);
            case DamageFalloffType.InverseSquare:
                return baseDamage * Mathf.Clamp01(1 - distanceFactor * distanceFactor); // not physically right, but seems to work more interesting
            default: return baseDamage; // placeholder line
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;

        Color color = Color.red;
        const int circles = 10;
        for(int i = 1; i <= circles; i++) {
            float r = scaledRadius * i / circles;
            Vector3 position = transform.position + new Vector3(r, 0, 0);
            float damage = CalculateDamage(position);
            color.a = damage / baseDamage;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, r);
            UnityEditor.Handles.Label(position, $"{damage:0}", style);
        }
    }
#endif
}
