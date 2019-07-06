using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;

namespace Forest {

#if UNITY_EDITOR
    [System.Serializable]
    public class CandidateTable : IEnumerable<Candidate> {
        [SerializeField] Candidate[] candidates = new Candidate[0];

        [Range(0, 1)]
        [SerializeField]
        float intensity = 1;

        [Button]
        private void NormalizeChances() {
            float sum = 0;
            foreach(Candidate candidate in candidates) {
                sum += candidate.chance;
            }
            if(sum == 0) return;

            foreach(Candidate candidate in candidates) {
                candidate.chance /= sum;
            }
        }

        public float GetIntensity() => intensity;

        IEnumerator<Candidate> IEnumerable<Candidate>.GetEnumerator() {
            return ((IEnumerable<Candidate>)candidates).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return candidates.GetEnumerator();
        }

        public GameObject GetRandomPrefab() {
            return GetRandomPrefab(this.intensity);
        }

        public GameObject GetRandomPrefab(float overridenIntensity) {
            if(overridenIntensity < 0 || overridenIntensity > 1)
                throw new System.ArgumentOutOfRangeException(nameof(overridenIntensity));

            if(candidates.Length == 0 || Random.value > overridenIntensity)
                return null;

            Candidate candidate = GetRandomCandidate();
            if(candidate == null)
                return null;

            return candidate.prefab;
        }

        private Candidate GetRandomCandidate() {
            float sum = 0;
            foreach(Candidate candidate in candidates) {
                sum += candidate.chance;
            }

            if(sum == 0) return null;

            float[] distributedChances = new float[candidates.Length];
            distributedChances[0] = candidates[0].chance / sum;
            for(int i = 1; i < distributedChances.Length; i++) {
                distributedChances[i] = candidates[i].chance / sum + distributedChances[i - 1];
            }
            distributedChances[distributedChances.Length - 1] = 1;

            float randValue = Random.value;
            for(int i = 0; i < candidates.Length; i++) {
                if(distributedChances[i] > randValue)
                    return candidates[i];
            }
            return null;
        }
    }
#endif

}