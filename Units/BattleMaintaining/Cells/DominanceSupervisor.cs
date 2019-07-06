using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public class DominanceSupervisor : ISavable {
        private Cell cell;

        public const float maxWeight = 7f;
        private float alliesTime, enemiesTime;

        public float alliesWeight => alliesTime;
        public float enemiesWeight => enemiesTime;

        public Team currentDominator => alliesTime > enemiesTime ? Team.Allies : Team.Enemies;

        public DominanceSupervisor(Cell cell) {
            this.cell = cell;
            if(cell.team == Team.Allies)
                alliesTime = maxWeight;
            else
                enemiesTime = maxWeight;
        }

        private void UpdateTime(ref float time, int sign) {
            time += sign * Time.deltaTime;
            time = Mathf.Clamp(time, 0, maxWeight);
        }

        public void Update() {
            int alliesSign = 1, enemiesSign = 1;
            if(cell.team == Team.Allies)
                enemiesSign = -1;
            else
                alliesSign = -1;

            UpdateTime(ref alliesTime, alliesSign);
            UpdateTime(ref enemiesTime, enemiesSign);
        }

        public object CaptureSnapshot() {
            return new Snapshot { alliesTime = alliesTime, enemiesTime = enemiesTime };
        }

        public void RestoreSnapshot(object data) {
            Snapshot snapshot = (Snapshot)data;
            alliesTime = snapshot.alliesTime;
            enemiesTime = snapshot.enemiesTime;
        }

        [System.Serializable]
        private struct Snapshot {
            public float alliesTime, enemiesTime;
        }
    }
}
