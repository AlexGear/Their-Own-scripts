using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public static class BMExtension {
        public static bool IsAllyOrEnemyObject(this GameObject gameObject, out Team team) {
            switch(gameObject.layer) {
                case Unit.alliesLayer: team = Team.Allies; return true;
                case Unit.enemiesLayer: team = Team.Enemies; return true;
                default: team = default(Team); return false;
            }
        }

        public static bool IsAlly(this GameObject gameObject) {
            return gameObject.layer == Unit.alliesLayer;
        }

        public static bool IsEnemy(this GameObject gameObject) {
            return gameObject.layer == Unit.enemiesLayer;
        }
    }
}