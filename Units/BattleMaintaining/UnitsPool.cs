using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public class UnitsPool {
        private BattleMaintainer battleMaintainer;

        public UnitsPool(BattleMaintainer battleMaintainer) {
            this.battleMaintainer = battleMaintainer;
        }

        public IEnumerable<GameObject> SpawnUnits(IEnumerable<GameObject> prefabs, ConflictLocation conflict) {
            Vector2 originalPos = conflict.center;
            foreach(var prefab in prefabs) {
                Vector2 position;
                if(!FindPositionForUnit(prefab, conflict, out position))
                    continue;
                
                GameObjectPool goPool = GameObjectPool.Get(prefab);
                GameObject instance = goPool.Take();

                SetupUnit(instance, position, goPool, originalPos);

                yield return instance;
            }
        }

        private static void SetupUnit(GameObject instance, Vector2 position, GameObjectPool goPool, Vector2 conflictCenter) {
            var unit = instance.GetComponent<Unit>();
            if(unit == null) {
                throw new System.Exception($"Trying to spawn a unit {instance.name} without Unit script");
            }

            if(unit.ai != null) {
                position += Random.insideUnitCircle * 9f;
                unit.ai.navAgent.SamplePosition(position, 9f, out position);
            }
            ForceSetPosition(instance, position);

            unit.usedInPool = true;
            if(unit.isDead)
                unit.Revive();

            System.Action<Unit> onDied = u => goPool.Release(u.gameObject);
            unit.Died -= onDied;
            unit.Died += onDied;
        }

        private static void ForceSetPosition(GameObject instance, Vector2 position) {
            instance.SetActive(false);
            instance.transform.position = position;
            instance.SetActive(true);
        }

        private bool FindPositionForUnit(GameObject go, ConflictLocation conflict, out Vector2 position) {
            Team team;
            if(!go.IsAllyOrEnemyObject(out team)) {
                throw new System.Exception($"Unit {go.name}'s layer is neither the Allies nor the Enemies");
            }
            if(!battleMaintainer.FindPositionOutOfSight(conflict, team, out position)) {
                //Debug.LogWarning($"Couldn't find a position out of sight for unit {go.name} around conflict {{{conflict}}}");
                return false;
            }
            return true;
        }
    }
}