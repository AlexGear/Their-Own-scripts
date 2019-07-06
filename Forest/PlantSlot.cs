using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Forest {

#if UNITY_EDITOR
    public class PlantSlot : MonoBehaviour {
        public Vector3 minScale;
        public Vector3 maxScale;
        public GameObject plantPrefabReference;
        public bool hasSwitchableCollider;

        private void Reset() {
            minScale = transform.localScale * 0.9f;
            maxScale = transform.localScale * 1.1f;
            TryFindAndAssignPrefab();
            AssignLODGroupRenderers();
        }

        [Button("Try to Find and Assign Prefab\n(Last Child in Hierarchy)")]
        private void TryFindAndAssignPrefab() {
            var potentialPrefabInstance = transform.GetChild(transform.childCount - 1).gameObject;
            plantPrefabReference = PrefabUtility.GetCorrespondingObjectFromSource(potentialPrefabInstance);
        }

        [Button("Assign LOD Group Renderers")]
        private void AssignLODGroupRenderers() {
            LODGroup lodGroup = GetComponent<LODGroup>();
            if(lodGroup == null) {
                Debug.LogWarning("No LODGroup Found", this);
                return;
            }

            LOD[] lods = lodGroup.GetLODs();
            if(lods.Length != 1) {
                Debug.LogError("LODGroup must have exactly 1 LOD", this);
                return;
            }
            lods[0].renderers = GetComponentsInChildren<Renderer>(true);
            lodGroup.SetLODs(lods);
        }

        public void Randomize() {
            Vector3 deltaScale = maxScale - minScale;
            transform.localScale = minScale + deltaScale * Random.value;
            transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
        }
    }
#endif

}