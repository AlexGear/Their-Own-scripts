using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest {

#if UNITY_EDITOR
    [System.Serializable]
    public class Candidate {
        public GameObject prefab;
        public float chance = 1;

        public Collider2D FindCollider() {
            if(prefab == null) {
                return null;
            }
            return prefab.GetComponentInChildren<Collider2D>();
        }
    }
#endif

}