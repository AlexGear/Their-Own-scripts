using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public struct ConflictLocation {
        public readonly Cell alliesCell;
        public readonly Cell enemiesCell;
        public readonly Vector2 center;

        public ConflictLocation(Cell cell1, Cell cell2) {
            if(cell1.dominantTeam == cell2.dominantTeam)
                throw new System.ArgumentException($"Invalid ConflictLocation. Expected different 'dominantTeam' values, but got the same {cell1.dominantTeam} from cells '{cell1}' and '{cell2}'");
            if(cell1.dominantTeam == Team.Allies) {
                alliesCell = cell1;
                enemiesCell = cell2;
            }
            else {
                alliesCell = cell2;
                enemiesCell = cell1;
            }
            center = (cell1.position + cell2.position) * 0.5f;
        }

        public override string ToString() {
            return $"Allies: {alliesCell} <=> Enemies: {enemiesCell}";
        }
    }
}
