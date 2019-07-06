using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class Zapurasser : BaseWeapon {
    [SerializeField]
    [NaughtyAttributes.MinMaxSlider(0, 2)]
    private Vector2 cooldownMultiplier = new Vector2(0.1f, 1f);

    [SerializeField] private float singleHitDamage = 1.5f;
    [SerializeField] private int maxHitTargets = 100;
    [SerializeField] private float maxCastDistance = 20f;
    [SerializeField] private uint additionalSweepCasts = 5;
    [SerializeField] private float additionalSweepCastsAngle = 30;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private Lightning lightning;
    [SerializeField] private Lightning secondLightning;
    [SerializeField] private float hitPointRandOffset = 1f;
    [SerializeField] private AudioSource dischargeAudioSource;
    [SerializeField] private AudioClip[] dischargeSounds;
    [SerializeField] private AudioSource passiveNoiseSource;
    [SerializeField] private AudioSource clickSoundSource;
    [SerializeField] private AudioSource zapSoundSource;
    [SerializeField] private AudioClip[] zapSounds;

    private Collider2D hitArea;
    private ContactFilter2D contactFilter;
    private Collider2D[] overlapResults;
    private int layerMask;
    private CircleCollider2D randomOffsetCollider;

    public override int ammo {
        get { return base.ammo; }

        set {
            base.ammo = value;
            passiveNoiseSource.mute = base.ammo <= 0;
        }
    }

    protected override void Start() {
        base.Start();
        hitArea = GetComponent<Collider2D>();
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(hitLayers);
        contactFilter.useTriggers = true;
        layerMask = hitLayers.value;
        overlapResults = new Collider2D[maxHitTargets];
        GameObject randomOffsetObject = new GameObject("Zapurasser Offset Probe");
        randomOffsetCollider = randomOffsetObject.AddComponent<CircleCollider2D>();
        randomOffsetCollider.radius = 0.001f;
        randomOffsetCollider.isTrigger = true;
    }

    private IEnumerable<RaycastHit2D> Cast() {
        Vector3 lightningStart = lightning.transform.position;

        // Targets within hitArea
        int targetCount = hitArea.OverlapCollider(contactFilter, overlapResults);
        for(int i = 0; i < targetCount; i++) {
            Transform targetTransform = overlapResults[i].transform;
            if(targetTransform == randomOffsetCollider.transform) {
                continue;
            }

            Vector3 direction = targetTransform.position - lightningStart;
            RaycastHit2D hit = Physics2D.Raycast(lightningStart, direction, maxCastDistance, layerMask);
            if(hit.collider?.transform == targetTransform) {
                yield return hit;
            }
        }

        // Additional pass gives a chance for a target to have more than 1 hit
        if(additionalSweepCasts == 0) {
            yield break;
        }
        float halfAngle = additionalSweepCastsAngle * 0.5f;
        for(int i = 0; i < additionalSweepCasts; i++) {
            float randAngle = Random.Range(-halfAngle, halfAngle);
            Vector2 dir = Quaternion.AngleAxis(randAngle, Vector3.forward) * lightning.transform.up;
            RaycastHit2D hit = Physics2D.Raycast(lightningStart, dir, maxCastDistance, layerMask);
            if(hit) {
                yield return hit;
            }
        }
    }

    private Vector3 OffsetEndRandomly(RaycastHit2D hit) {
        Vector2 sideOffset = Vector2.Perpendicular(hit.normal) * Random.Range(-hitPointRandOffset, hitPointRandOffset);
        Vector2 forwOffset = hit.normal * Random.Range(0.1f, hitPointRandOffset);
        randomOffsetCollider.transform.position = hit.point + sideOffset + forwOffset;
        Vector3 point = hit.collider.Distance(randomOffsetCollider).pointA;

        float maxDistanceWithReserve = maxCastDistance * 1.2f;
        bool notTooFar = (point - lightning.transform.position).CompareLength(maxDistanceWithReserve) < 0;
        if(notTooFar) {
            return point;
        }
        return hit.point;
    }

    protected override void Fire() {
        base.Fire();
        cooldownTimer *= Random.Range(cooldownMultiplier.x, cooldownMultiplier.y);

        IEnumerable<RaycastHit2D> hits = Cast();

        var ends = new List<Vector3>();
        var damagedUnits = new HashSet<Unit>();
        foreach(var hit in hits) {
            ends.Add(OffsetEndRandomly(hit));

            Unit unit = hit.transform.GetComponentInParent<Unit>();
            if(unit != null && !damagedUnits.Contains(unit)) {
                unit.ApplyDamage(singleHitDamage, owner);
                damagedUnits.Add(unit);
            }
        }

        Transform lightningTransform = lightning.transform;
        bool noHit = false;
        if(ends.Count == 0) {
            noHit = true;
            Vector3 forwardShift = lightningTransform.up * Random.Range(1f, 4f);
            Vector3 sideShift = lightningTransform.right * Random.Range(-2f, 2f);
            ends.Add(lightningTransform.position + forwardShift + sideShift);
        }
        var ends1 = new List<Vector3>();
        var ends2 = new List<Vector3>();
        foreach(var end in ends) {
            if(Random.value < 0.5f) {
                ends1.Add(end);
                ends2.Add(end);
            }
            else {
                (Random.value < 0.5f ? ends1 : ends2).Add(end);
            }
        }
        bool noHit1 = noHit || ends1.Count == 0;
        bool noHit2 = noHit || ends2.Count == 0;
        lightning.GenerateAsRoot(lightningTransform.position, ends1, noHit1);
        secondLightning.GenerateAsRoot(lightningTransform.position, ends2, noHit2);

        StartCoroutine(Strike());

        bool noAmmo = ammo <= 0;
        shotAudioSource.mute = noAmmo;
        clickSoundSource.mute = noAmmo || noHit;
        dischargeAudioSource.PlayOneShot(dischargeSounds.GetRandomItem());
        if(!noHit) {
            zapSoundSource.transform.position = ends[0];
            zapSoundSource.PlayOneShot(zapSounds.GetRandomItem());
        }
    }

    private IEnumerator Strike() {
        secondLightning.Strike();
        yield return new WaitForSeconds(secondLightning.GetShowDuration() * 0.2f);
        lightning.Strike();
        yield return new WaitForSeconds(secondLightning.GetShowDuration() * 0.8f);
        if(Random.value < 0.5f)
            secondLightning.Strike();
        else
            lightning.Strike();

        if(Random.value < 0.65f) {
            yield return new WaitForSeconds(secondLightning.GetShowDuration() * 0.8f);
            secondLightning.Strike();
        }
    }

    protected override void Update () {
        base.Update();
        if(!isTriggerPressed || !isFireLineUnlocked) {
            shotAudioSource.mute = true;
            clickSoundSource.mute = true;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Handles.color = new Color(1, 0.8f, 0, 0.2f);
        var from = Quaternion.AngleAxis(-additionalSweepCastsAngle * 0.5f, Vector3.forward) * lightning.transform.up;
        Handles.DrawSolidArc(lightning.transform.position, Vector3.forward, from, 
            additionalSweepCastsAngle, maxCastDistance);
    }
#endif
}
