using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [RequireComponent(typeof(Collider2D))]
    public class UnitSet : MonoBehaviour {
        [SerializeField] int maintainAlliesNumber = 5;
        [SerializeField] int maintainEnemiesNumber = 5;
        [SerializeField] GameObject[] _alliesPrefabs = new GameObject[0];
        [SerializeField] GameObject[] _enemiesPrefabs = new GameObject[0];

        private Collider2D trigger;

        public bool OverlapPoint(Vector2 point) => trigger.OverlapPoint(point);

        private void Awake() {
            trigger = GetComponent<Collider2D>();
        }

        public IEnumerable<GameObject> GetUnitsToSpawnAroundConflict(ConflictLocation conflict, float radius) {
            var alliesThere = Unit.GetInRadius<Unit>(conflict.center, radius, 1 << Unit.alliesLayer);
            var enemiesThere = Unit.GetInRadius<Unit>(conflict.center, radius, 1 << Unit.enemiesLayer);
            
            int addAllies = maintainAlliesNumber - alliesThere.Count;
            int addEnemies = maintainEnemiesNumber - enemiesThere.Count;
            
            for(int i = 0; i < addAllies; i++) {
                yield return _alliesPrefabs.GetRandomItem();
            }
            for(int i = 0; i < addEnemies; i++) {
                yield return _enemiesPrefabs.GetRandomItem();
            }
        }
    }
}