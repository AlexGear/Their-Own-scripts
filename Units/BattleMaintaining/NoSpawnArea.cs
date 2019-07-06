using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [RequireComponent(typeof(Collider2D))]
    public class NoSpawnArea : MonoBehaviour {
        private Collider2D trigger;

        private static HashSet<NoSpawnArea> areas = new HashSet<NoSpawnArea>();

        private void Awake() {
            trigger = GetComponent<Collider2D>();
        }

        private void OnEnable() {
            areas.Add(this);
        }

        private void OnDisable() {
            areas.Remove(this);
        }

        public bool CanSpawn(Vector2 point) => !trigger.OverlapPoint(point);

        public static bool CanSpawnAtPoint(Vector2 point) => areas.All(x => x.CanSpawn(point));
    }
}