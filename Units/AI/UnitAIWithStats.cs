using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public class UnitAIWithStats : UnitAI {
        public UnitStats stats { get; protected set; }

        public UnitAIWithStats(Unit owner, NavMeshAgent navAgent, UnitStats stats) 
            : base(owner, navAgent) 
        {
            this.stats = stats;
        }
    }
}