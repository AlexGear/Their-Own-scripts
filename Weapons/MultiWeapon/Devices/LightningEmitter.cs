using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class LightningEmitter : MonoBehaviour
    {
        [SerializeField] Collider2D hitArea;
        [SerializeField] float singleHitDamage = 1.5f;
        [SerializeField] int maxHitTargets = 100;
        [SerializeField] float maxCastDistance = 20f;
        [SerializeField] LayerMask hitMask;
        [SerializeField] Lightning lightning;
        [SerializeField] Lightning secondLightning;
        [SerializeField] float hitPointRandOffset = 1f;
        [SerializeField] AudioSource buzzAudio;
        [SerializeField] AudioSource dischargeAudio;
        [SerializeField] AudioClip[] dischargeSounds;
        [SerializeField] AudioSource clickAudio;
        [SerializeField] AudioSource zapAudio;
        [SerializeField] AudioClip[] zapSounds;

        private int strikeSoundUnmuters = 0;
        private ContactFilter2D contactFilter;
        private Collider2D[] overlapResults;
        private CircleCollider2D randomOffsetCollider;

        private void Awake()
        {
            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(hitMask);
            contactFilter.useTriggers = true;
            overlapResults = new Collider2D[maxHitTargets];
            GameObject randomOffsetObject = new GameObject("LightningEmitter Offset Probe");
            randomOffsetCollider = randomOffsetObject.AddComponent<CircleCollider2D>();
            randomOffsetCollider.radius = 0.001f;
            randomOffsetCollider.isTrigger = true;
        }

        private IEnumerable<RaycastHit2D> Cast()
        {
            Vector3 start = transform.position;

            // Targets within hitArea
            int targetCount = hitArea.OverlapCollider(contactFilter, overlapResults);
            for (int i = 0; i < targetCount; i++)
            {
                Transform targetTransform = overlapResults[i].transform;
                if (targetTransform == randomOffsetCollider.transform)
                {
                    continue;
                }

                Vector3 direction = targetTransform.position - start;
                RaycastHit2D hit = Physics2D.Raycast(start, direction, maxCastDistance, hitMask);
                if (hit.collider?.transform == targetTransform)
                {
                    yield return hit;
                }
            }
        }

        private Vector3 OffsetEndRandomly(RaycastHit2D hit)
        {
            Vector2 sideOffset = Vector2.Perpendicular(hit.normal) * Random.Range(-hitPointRandOffset, hitPointRandOffset);
            Vector2 forwOffset = hit.normal * Random.Range(0.1f, hitPointRandOffset);
            randomOffsetCollider.transform.position = hit.point + sideOffset + forwOffset;
            Vector3 point = hit.collider.Distance(randomOffsetCollider).pointA;

            float maxDistanceWithReserve = maxCastDistance * 1.2f;
            bool notTooFar = (point - transform.position).CompareLength(maxDistanceWithReserve) < 0;
            if (notTooFar)
            {
                return point;
            }
            return hit.point;
        }

        public void Fire(Unit damageSource)
        {
            IEnumerable<RaycastHit2D> hits = Cast();

            var ends = new List<Vector3>();
            var damagedUnits = new HashSet<Unit>();
            foreach (var hit in hits)
            {
                ends.Add(OffsetEndRandomly(hit));

                Unit unit = hit.transform.GetComponentInParent<Unit>();
                if (unit != null && !damagedUnits.Contains(unit))
                {
                    unit.ApplyDamage(singleHitDamage, damageSource);
                    damagedUnits.Add(unit);
                }
            }

            bool noHit = false;
            if (ends.Count == 0)
            {
                noHit = true;
                Vector3 forwardShift = transform.up * Random.Range(2.1f, 2.8f);
                Vector3 sideShift = transform.right * Random.Range(-0.2f, 0.2f);
                ends.Add(transform.position + forwardShift + sideShift);
            }
            var ends1 = new List<Vector3>();
            var ends2 = new List<Vector3>();
            foreach (var end in ends)
            {
                if (Random.value < 0.5f)
                {
                    ends1.Add(end);
                    ends2.Add(end);
                }
                else
                {
                    (Random.value < 0.5f ? ends1 : ends2).Add(end);
                }
            }
            bool noHit1 = noHit || ends1.Count == 0;
            bool noHit2 = noHit || ends2.Count == 0;
            lightning.GenerateAsRoot(transform.position, ends1, noHit1);
            secondLightning.GenerateAsRoot(transform.position, ends2, noHit2);

            StartCoroutine(Strike(noHit));

            dischargeAudio.PlayOneShot(dischargeSounds.GetRandomItem());
            if (!noHit)
            {
                zapAudio.transform.position = ends[0];
                zapAudio.PlayOneShot(zapSounds.GetRandomItem());
            }
        }

        private IEnumerator Strike(bool noHit)
        {
            buzzAudio.mute = false;
            clickAudio.mute = noHit;
            strikeSoundUnmuters++;

            secondLightning.Strike();
            yield return new WaitForSeconds(secondLightning.GetShowDuration() * 0.2f);
            lightning.Strike();
            yield return new WaitForSeconds(secondLightning.GetShowDuration() * 0.8f);
            if (Random.value < 0.5f)
                secondLightning.Strike();
            else
                lightning.Strike();

            if (Random.value < 0.65f)
            {
                yield return new WaitForSeconds(secondLightning.GetShowDuration() * 0.8f);
                secondLightning.Strike();
            }

            if (--strikeSoundUnmuters == 0)
            {
                buzzAudio.mute = true;
                clickAudio.mute = true;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = new Color(1, 0.8f, 0, 0.2f);
            var from = Quaternion.AngleAxis(-10, Vector3.forward) * transform.up;
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.forward, from,
                20, maxCastDistance);
        }
#endif
    }
}