using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest {
    
    public class PlantPool {
        private class ConcretePlantPool {
            private class Entry {
                public readonly GameObject gameObject;
                public readonly Collider2D collider;

                private static Transform _root;
                private static Transform root => _root != null ? _root : (_root = new GameObject("PlantPool").transform);

                public Entry(GameObject prefab, bool hasSwitchableCollider) {
                    this.gameObject = Object.Instantiate(prefab, root);
                    if(hasSwitchableCollider) {
                        this.collider = gameObject.GetComponent<Collider2D>();
                    }
                }
            }

            private readonly GameObject prefab;
            private readonly bool hasSwitchableCollider;

            private Stack<Entry> free;
            private Dictionary<SlotData, Entry> used;

            public ConcretePlantPool(GameObject prefab, bool hasSwitchableCollider) {
                this.prefab = prefab;
                this.hasSwitchableCollider = hasSwitchableCollider;

                free = new Stack<Entry>();
                used = new Dictionary<SlotData, Entry>();
            }

            public void PlacePlant(SlotData slotData) {
                Entry entry;
                if(free.Count != 0)
                    entry = free.Pop();
                else
                    entry = new Entry(slotData.prefab, hasSwitchableCollider);

                used[slotData] = entry;

                Transform transform = entry.gameObject.transform;
                transform.position = slotData.position;
                transform.rotation = slotData.rotation;
                transform.localScale = slotData.scale;

                if(hasSwitchableCollider) {
                    entry.collider.enabled = slotData.isSwitchableColliderEnabled;
                    entry.gameObject.tag = !string.IsNullOrEmpty(slotData.tag) ? slotData.tag : prefab.tag;
                }
                entry.gameObject.SetActive(true);
            }

            public void RemovePlant(SlotData slotData) {
                if(!used.TryGetValue(slotData, out Entry entry)) {
                    return;
                }
                used.Remove(slotData);
                free.Push(entry);
                if(hasSwitchableCollider && slotData.isSwitchableColliderEnabled) {
                    entry.collider.enabled = false;
                }
                entry.gameObject.SetActive(false);
            }
        }

        private readonly string outlineBushesTag;

        // Key - prefab
        private Dictionary<GameObject, ConcretePlantPool> concretePools 
            = new Dictionary<GameObject, ConcretePlantPool>();
        
        public void PlacePlant(SlotData slotData) {
            ConcretePlantPool concretePool;
            if(!concretePools.TryGetValue(slotData.prefab, out concretePool)) {
                concretePool = new ConcretePlantPool(slotData.prefab, slotData.hasSwitchableCollider);
                concretePools.Add(slotData.prefab, concretePool);
            }
            concretePool.PlacePlant(slotData);
        }

        public void RemovePlant(SlotData slotData) {
            if(concretePools.TryGetValue(slotData.prefab, out var concretePool)) {
                concretePool.RemovePlant(slotData);
            }
        }
    }

}