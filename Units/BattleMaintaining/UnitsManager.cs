using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [System.Serializable]
    public class UnitsManager {
        [SerializeField] Unit[] vitalAllies = new Unit[0];
        [SerializeField] UnitSet[] unitSets = new UnitSet[0];

        public event System.Action<Unit> VitalAllyDied;

        public void OnStart() {
            foreach(var vitalAlly in vitalAllies) {
                vitalAlly.usedInPool = true;
                vitalAlly.Died += u => VitalAllyDied?.Invoke(u);
            }
        }

        public UnitSet GetUnitSetAt(Vector2 position) {
            foreach(var unitSet in unitSets) {
                if(unitSet.OverlapPoint(position))
                    return unitSet;
            }
            return null;
        }
    }
}