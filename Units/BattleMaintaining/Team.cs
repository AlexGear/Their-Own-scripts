using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public enum Team { Enemies, Allies }

    public static class TeamExt {
        public static Team Opposite(this Team team) {
            return team == Team.Allies ? Team.Enemies : Team.Allies;
        }
    }
}
