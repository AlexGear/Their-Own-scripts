using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public class AlliesPersistentCell : Cell {
        public override Team team {
            get { return Team.Allies; }
            set { }
        }

        // No need to store any data about persistent cells
        public override object CaptureSnapshot() => null;

        public override void RestoreSnapshot(object data) {
        }
    }
}