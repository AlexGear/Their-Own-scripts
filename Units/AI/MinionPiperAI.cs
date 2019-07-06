using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public class MinionPiperAI : UnitAI {
        private MinionPiper minion;

        public override FollowPath followPath { get => null; set { } }
        public override FollowPath secondaryFollowPath { get => null; set { } }

        public void FollowPipe() {
            if(PipeOfSatan.current != null) {
                if(!(state is MoveToPositionState)) {
                    state = new FollowObjectState(this, PipeOfSatan.current.transform, keepDistance: 15f);
                }
            }
            else {
                Debug.LogError("No PipeOfSatan found", owner);
            }
        }

        public MinionPiperAI(MinionPiper unit, NavMeshAgent navAgent) 
            : base(unit, navAgent) 
        {
            this.minion = unit;
        }
    }
}