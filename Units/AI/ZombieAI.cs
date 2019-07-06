using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public class ZombieAI : UnitAIWithTarget {
        public override bool isMelee => true;

        public ZombieAI(Unit unit, NavMeshAgent navAgent, ZombieStats stats) 
            : base(unit, navAgent, stats) 
        {
            state = new TargetSearchState(this);
        }

        public void StartAttack() {
            (owner as Zombie).StartAttack();
        }

        public override void OnDamageTaken(float value, Unit source) {
            Vector2 look = source != null ? (source.position - owner.position) : owner.forward;
            state = new DamagedState(this, look);
        }

        public override void ContinueTargetPursuing() {
            if(target.Is()) {
                if(!(state is ZombiePursuingTarget)) {
                    state = new ZombiePursuingTarget(this);
                }
            }
            else {
                state = new TargetSearchState(this);
            }
        }

        public override void AttractAttention(Vector2 position) {
            if(!IsInCombat(true)) {
                state = new TargetSearchWanderingState(this, position, false);
            }
        }
    }
}