using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public class SightedUnitAI : UnitAIWithStats {
        public readonly Vision vision;

        public SightedUnitAI(Unit owner, NavMeshAgent navAgent, UnitStats stats) : 
            base(owner, navAgent, stats) {
            vision = new Vision(this);
        }

        public override void Think() {
            base.Think();

            vision.Think();
        }

        public override string ToString() {
            return $"Vision: { vision}\n" + base.ToString();
        }
    }
}