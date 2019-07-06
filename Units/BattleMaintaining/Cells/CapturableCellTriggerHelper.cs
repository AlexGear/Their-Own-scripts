using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public class CapturableCellTriggerHelper {
        private Collider2D trigger;

        private List<Unit> touchingAllies = new List<Unit>();
        private List<Unit> touchingEnemies = new List<Unit>();

        public int alliesCount => touchingAllies.Count;
        public int enemiesCount => touchingEnemies.Count;

        public CapturableCellTriggerHelper(Collider2D trigger) {
            this.trigger = trigger;
        }

        public int UnitsCount(Team team) {
            return team == Team.Allies ? alliesCount : enemiesCount;
        }

        public void OnTriggerEnter(Collider2D collision) {
            Team team;
            if(!collision.gameObject.IsAllyOrEnemyObject(out team))
                return;

            Unit unit = collision.GetComponentInParent<Unit>();
            if(unit == null)
                return;

            if(team == Team.Allies)
                touchingAllies.Add(unit);
            else
                touchingEnemies.Add(unit);
        }

        private bool IsNotTouching(Unit unit) {
            return !(unit != null && unit.collider != null && unit.isActiveAndEnabled && unit.collider.IsTouching(trigger));
        }

        public void Update() {
            if(touchingAllies.Count != 0) {
                touchingAllies.RemoveAll(IsNotTouching);
            }
            if(touchingEnemies.Count != 0) {
                touchingEnemies.RemoveAll(IsNotTouching);
            }
        }
    }
}
