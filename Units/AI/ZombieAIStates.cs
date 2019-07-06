using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class ZombiePursuingTarget : IntervalThinkState {
        protected UnitAIWithTarget ai;
        protected const float minDistanceToLoseTarget = 4f;
        protected Vector2 destination;

        public ZombiePursuingTarget(UnitAIWithTarget ai) : base(interval: 0.2f) {
            this.ai = ai;
        }

        public override void Enter() {
            if(!ai.target.Is()) {
                ai.state = new TargetSearchState(ai);
                return;
            }

            ai.target.Died += OnTargetDied;
            IntervalThink();
        }

        public override void Leave() {
            if(ai.target != null)
                ai.target.Died -= OnTargetDied;
        }

        public override void IntervalThink() {
            if(!ai.target.Is()) {
                ai.state = new TargetSearchState(ai);
                return;
            }
            if(ai.isTargetVisible) {
                ThinkTargetVisible();
            }
            else {
                ThinkTargetHidden();
            }
        }

        protected virtual void ThinkTargetVisible() {
            if(!TryToAttackTargetIfNearby()) {
                MoveTo(ai.targetPosition);
            }
        }

        protected virtual void ThinkTargetHidden() {
            bool isAlreadyMovingToLastTargetPosition = destination == ai.lastTargetPosition;
            if(!isAlreadyMovingToLastTargetPosition) {
                MoveTo(ai.lastTargetPosition);
            }
            else if(CheckIfTargetIsLost()) {
                ai.state = new TargetSearchWanderingState(ai, ai.lastTargetPosition, true);
            }
        }

        protected virtual bool TryToAttackTargetIfNearby() {
            var targetPosition = ai.targetPosition;
            float attackRange = ((ZombieStats)ai.stats).attackRange;
            var toTarget = targetPosition - ai.owner.position;
            if(toTarget.CompareLength(attackRange) < 0 && ai.isTargetVisible) {
                ai.state = new ZombieAttacking((ZombieAI)ai);
                return true;
            }
            return false;
        }

        protected virtual bool CheckIfTargetIsLost() {
            if(ai.isTargetVisible) {
                return false;
            }
            return ai.navAgent.ReachedDestination(minDistanceToLoseTarget);
        }

        protected virtual void MoveTo(Vector2 position) {
            destination = position;
            if(!ai.navAgent.pathPending)
                ai.navAgent.SetDestination2D(destination);
        }

        protected virtual void OnTargetDied(Unit target) {
            ai.state = new TargetSearchState(ai);
        }
    } 

    public class ZombieAttacking : AIState {
        private ZombieAI ai;

        public ZombieAttacking(ZombieAI ai) {
            this.ai = ai;
        }

        public override void Enter() {
            ai.StartAttack();
            MoveTo(ai.targetPosition);
        }

        private void MoveTo(Vector2 targetPosition) {
            Vector2 outwardsDir = (ai.owner.position - targetPosition).normalized;
            var stats = (ZombieStats)ai.stats;
            targetPosition += outwardsDir * (stats.attackRange - stats.attackAreaRadius);
            ai.navAgent.SetDestination2D(targetPosition);
        }

        public override void Think() {
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, ai.targetPosition - ai.owner.position);
            ai.owner.rotation = Quaternion.Lerp(ai.owner.rotation, rotation, Time.deltaTime * 10f);
        }
    }
}