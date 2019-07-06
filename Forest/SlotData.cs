using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest {

    [System.Serializable]
    public class SlotData {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public GameObject prefab;
#if UNITY_EDITOR
        // Used to restore slot objects from SlotData's in Forest script
        public GameObject slotObjectPrefab;
#endif

        public string tag;

        public bool hasSwitchableCollider;
        public bool isSwitchableColliderEnabled;

        public SlotData() {
        }

        public SlotData(SlotData other) {
            this.position = other.position;
            this.rotation = other.rotation;
            this.scale = other.scale;

            this.prefab = other.prefab;
#if UNITY_EDITOR
            this.slotObjectPrefab = other.slotObjectPrefab;
#endif

            this.tag = other.tag;

            this.hasSwitchableCollider = other.hasSwitchableCollider;
            this.isSwitchableColliderEnabled = other.isSwitchableColliderEnabled;
        }
    }

}