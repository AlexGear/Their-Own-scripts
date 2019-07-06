using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public class SoldierPursuingTarget : ZombiePursuingTarget {
        private new SoldierAI ai;
        private SoldierStats stats;

        public SoldierPursuingTarget(SoldierAI ai) : base(ai) {
            this.ai = ai;
            this.stats = (SoldierStats)ai.stats;
        }

        protected override void ThinkTargetVisible() {
            ai.alwaysLookAtTarget = true;
            if(!TryToAttackTargetIfNearby()) {
                ComeCloserToTarget();
            }
        }

        private void ComeCloserToTarget() {
            Vector3 position = ai.owner.position;
            Vector3 targetPosition = ai.lastTargetPosition;
            Vector3 toTargetDirection = (targetPosition - position).normalized;
            float distance = stats.fireDistance * 0.9f;
            Vector3 moveTo;
            do {
                moveTo = position + toTargetDirection * distance;
                if(ai.vision.WouldSeeUnitIgnoringFOVFrom(moveTo, ai.target)) {
                    break;
                }
                distance -= stats.fireDistance * 0.25f;
            } while(distance > stats.minTargetDistance);

            MoveTo(moveTo);
        }

        protected override bool TryToAttackTargetIfNearby() {
            var toTarget = ai.owner.position - ai.targetPosition;
            if(toTarget.CompareLength(stats.fireDistance) < 0 && CheckStraightVision()) {
                ai.state = new SoldierAttacking(ai);
                return true;
            }
            return false;
        }

        private bool CheckStraightVision() {
            return ai.vision.WouldSeeUnitFrom(ai.weapon.bulletStartPoint, ai.target, true);
        }

        protected override void ThinkTargetHidden() {
            ai.alwaysLookAtTarget = false;
            bool isAlreadyMovingToLastTargetPosition = destination == ai.lastTargetPosition;
            if(!isAlreadyMovingToLastTargetPosition) {
                MoveTo(ai.lastTargetPosition);
            }
            else if(CheckIfTargetIsLost()) {
                ai.state = new TargetSearchWanderingState(ai, ai.lastTargetPosition, true);
            }
        }

        public override void Leave() {
            ai.alwaysLookAtTarget = false;
            base.Leave();
        }
    }

    public class SoldierAttacking : IntervalThinkState {
        private SoldierAI ai;
        private SoldierStats stats;
        private Timer fireTimer;
        private float fireDuration => Random.Range(0.8f, 1.3f);
        private float firePause => Random.Range(0.4f, 1f);
        private Timer dodgingTimer;
        private int repositioningDirection = 1;

        public SoldierAttacking(SoldierAI ai) : base(0.2f) {
            this.ai = ai;
            this.stats = (SoldierStats)ai.stats;
        }

        public override void Enter() {
            dodgingTimer = new Timer(1f);
            ai.alwaysLookAtTarget = true;
            ai.weapon.isTriggerPressed = true;
            fireTimer = new Timer(fireDuration);

            TakePosition();
        }

        private void TakePosition() {
            const float offsetDistance = 4f;

            Vector2 position = ai.owner.position;
            Vector2 targetPosition = ai.target.position;
            Vector2 toTarget = targetPosition - position;
            float distanceToTarget = toTarget.magnitude;
            if(distanceToTarget < stats.fireDistance) {
                toTarget = toTarget / distanceToTarget * stats.fireDistance;
                distanceToTarget = stats.fireDistance;
            }
            Vector2 perpendicular = Vector2.Perpendicular(toTarget / distanceToTarget) * offsetDistance;
            position = targetPosition - toTarget;
            Vector2 moveTo = position + perpendicular;
            if(ai.vision.WouldSeeUnitIgnoringFOVFrom(moveTo, ai.target, true)) {
                ai.navAgent.SetDestination2D(moveTo);
                return;
            }
            moveTo = position - perpendicular;
            if(ai.vision.WouldSeeUnitIgnoringFOVFrom(moveTo, ai.target, true)) {
                ai.navAgent.SetDestination2D(moveTo);
                return;
            }
            ai.navAgent.SetDestination2D(position + (Vector2)ai.navAgent.velocity.normalized * 2);
        }

        public override void Think() {
            if(fireTimer.Tick()) {
                bool wasTriggerPressed = ai.weapon.isTriggerPressed;
                ai.weapon.isTriggerPressed = !wasTriggerPressed;
                if(wasTriggerPressed) {
                    if((ai.owner.health < ai.owner.maxHealth * 0.5f || 
                        Random.value < 0.3f) && !stats.disallowHiding)
                    {
                        ai.state = new SoldierHide(ai);
                        return;
                    }
                    fireTimer.interval = firePause;
                    if(IsTargetTooClose()) {
                        ai.state = new SoldierRetreat(ai);
                        return;
                    }
                    else if(IsTargetTooFar()) {
                        ai.ContinueTargetPursuing();
                        return;
                    }
                }
                else {
                    fireTimer.interval = fireDuration;
                }
            }
            base.Think();
        }

        private bool IsTargetTooClose() {
            return (ai.lastTargetPosition - ai.owner.position).CompareLength(stats.minTargetDistance) < 0;
        }

        private bool IsTargetTooFar() {
            return (ai.lastTargetPosition - ai.owner.position).CompareLength(stats.fireDistance) > 0;
        }

        private bool IsTargetExtremelyClose() {
            return (ai.lastTargetPosition - ai.owner.position).CompareLength(stats.minTargetDistance * 0.75f) < 0;
        }

        public override void IntervalThink() {
            if(ai.target.Is()) {
                Vector2 targetPosition = ai.target.position;
                Vector2 toTarget = targetPosition - ai.owner.position;
                float distanceToTarget = toTarget.magnitude;
                if(distanceToTarget < stats.minTargetDistance) {
                    Vector2 moveTo = targetPosition - toTarget / distanceToTarget *
                        (stats.minTargetDistance + stats.fireDistance) * 0.5f;
                    ai.navAgent.SetDestination2D(moveTo, true);
                }
            }

            if(ai.vision.WouldSeeUnitFrom(ai.weapon.bulletStartPoint, ai.target, true)) {
                if(IsTargetExtremelyClose()) {
                    ai.state = new SoldierHide(ai);
                    return;
                }
                if(dodgingTimer.Tick()) {
                    Dodging();
                }
            }
            else {
                ai.ContinueTargetPursuing();
            }
        }

        public void Dodging() {
            Vector2 position = ai.owner.position;
            Vector2 toTarget = ai.targetPosition - position;
            Vector2 perpendicular = Vector2.Perpendicular(toTarget) * repositioningDirection;
            if(ai.navAgent.CanReach(position + perpendicular, out var path)) {
                ai.navAgent.SetPath2D(path);
            }
            else {
                repositioningDirection = -repositioningDirection;
                ai.navAgent.SetDestination2D(position - perpendicular);
            }
        }

        public override void Leave() {
            ai.alwaysLookAtTarget = false;
            ai.weapon.isTriggerPressed = false;
        }

        public override string ToString() {
            return base.ToString() + $"\nFire Timer: {fireTimer}";
        }
    }

    public class SoldierRetreat : IntervalThinkState {
        private SoldierAI ai;
        private SoldierStats stats;
        private Unit fearSource;
        private Timer stopTimer;

        private Vector2 fearPosition => fearSource != null ? fearSource.position : ai.targetPosition;

        public SoldierRetreat(SoldierAI ai, Unit fearSource = null) : base(interval: 0.2f) {
            this.ai = ai;
            this.stats = (SoldierStats)ai.stats;
            this.fearSource = fearSource;
        }

        public override void Enter() {
            Vector2 position = ai.owner.position;
            float distance = stats.minTargetDistance * 1.3f;
            Vector2 moveTo = position + (position - fearPosition).normalized * distance;

            ai.alwaysLookAtTarget = true;
            ai.navAgent.SetDestination2D(moveTo, true);

            stopTimer = new Timer(0.9f * distance / ai.navAgent.speed);
        }

        public override void IntervalThink() {
            if(stopTimer.Tick()) {
                Stop();
                return;
            }
            ai.weapon.isTriggerPressed = ai.isTargetVisible && IsTargetTooClose();
        }

        private bool IsTargetTooClose() {
            return (ai.targetPosition - ai.owner.position).CompareLength(stats.minTargetDistance) < 0;
        }

        private void Stop() {
            ai.alwaysLookAtTarget = false;
            ai.weapon.isTriggerPressed = false;
            ai.ContinueTargetPursuing();
        }

        public override string ToString() {
            return base.ToString() + $"\nStop Timer: {stopTimer}";
        }
    }

    public class SoldierHide : IntervalThinkState {
        private static Collider2D[] overlapResults = new Collider2D[20];
        private const float obstacleSearchRadius = 20f;

        private SoldierAI ai;
        private SoldierStats stats;
        private Unit fearSource;
        private Vector2 start;
        private Timer waitTimer;
        private const float boost = 1.3f;
        public bool waiting { get; private set; } = false;

        public SoldierHide(SoldierAI ai, Unit fearSource = null) : base(0.4f) {
            this.ai = ai;
            this.stats = (SoldierStats)ai.stats;
            this.fearSource = fearSource;
        }

        public override void Enter() {
            if(((SoldierStats)ai.stats).disallowHiding) {
                ai.state = new TargetSearchState(ai);
                return;
            }
            ai.navAgent.speed *= boost;
            ai.navAgent.acceleration *= boost;

            if(FindCover(out var path)) {
                start = ai.owner.position;
                bool hadPath = ai.navAgent.hasPath;
                ai.navAgent.SetPath2D(path);
                if(hadPath && !ai.navAgent.hasPath) {
                    ai.state = new SoldierRetreat(ai, fearSource);
                    return;
                }
                ai.alwaysLookAtTarget = true;
            }
            else {
                ai.state = new SoldierRetreat(ai, fearSource);
            }
        }

        private bool FindCover(out NavMeshPath path) {
            path = null;

            Vector2 ownerPosition = ai.owner.position;
            int n = Physics2D.OverlapCircleNonAlloc(ownerPosition, obstacleSearchRadius, overlapResults, stats.bulletObstacles);
            if(n == 0) {
                return false;
            }

            bool found = false;
            float minSqrDistance = float.PositiveInfinity;
            Collider2D ownerCollider = ai.collider;
            Vector2 fearPosition = fearSource != null ? fearSource.position : ai.targetPosition;
            for(int i = 0; i < n; i++) {
                Vector2 safePlace = FindPossibleSafePlace(ownerCollider, overlapResults[i], fearPosition);
                if(!IsPlaceSafe(safePlace, fearPosition)) {
                    continue;
                }
                if(!ai.navAgent.GetTraversablePosition(safePlace, out var traversablePlace, 2f)) {
                    continue;
                }
                if(!ai.navAgent.CanReach(traversablePlace, out var pathToCurrentSafePlace)) {
                    continue;
                }
                float sqrDistance = (traversablePlace - ownerPosition).sqrMagnitude;
                if(sqrDistance < minSqrDistance) {
                    minSqrDistance = sqrDistance;
                    path = pathToCurrentSafePlace;
                    found = true;
                }
            }
            return found;
        }

        private Vector2 FindPossibleSafePlace(Collider2D ownerCollider, Collider2D obstacle, Vector2 fearPosition) {
            ColliderDistance2D distance2D = ownerCollider.Distance(obstacle);
            Vector2 obstaclePosition = obstacle.transform.position;
            Vector2 closestObstaclePoint = distance2D.pointB;
            Vector2 enemyFireVec = (closestObstaclePoint - fearPosition).normalized;
            Vector2 enemyFirePerpendicular = Vector2.Perpendicular(enemyFireVec);

            Vector2 place1 = closestObstaclePoint + enemyFireVec + enemyFirePerpendicular;
            Vector2 place2 = closestObstaclePoint + enemyFireVec - enemyFirePerpendicular;

            bool which = (place1 - obstaclePosition).sqrMagnitude < (place2 - obstaclePosition).sqrMagnitude;
            return which ? place1 : place2;
        }

        private bool IsPlaceSafe(Vector2 position, Vector2 fearPosition) {
            var hit = Physics2D.Linecast(position, fearPosition, stats.navHindrances);
            return hit.collider != null && hit.distance < 2f;
        }

        public override void Think() {
            base.Think();
            bool fire = ai.isTargetVisible && IsTargetInFireRange();
            ai.weapon.isTriggerPressed = fire;
        }

        public override void IntervalThink() {
            if(waiting) {
                if(ai.isTargetVisible) {
                    ai.state = new SoldierAttacking(ai);
                    return;
                }
                if(waitTimer.Tick()) {
                    ai.state = new TargetSearchWanderingState(ai, start, false);
                    //ai.alwaysLookAtTarget = true;
                }
            }
            else {
                if(ai.navAgent.ReachedDestination(1f)) {
                    HideAndWait();
                }
            }
        }

        private void HideAndWait() {
            ai.alwaysLookAtTarget = false;
            waiting = true;
            waitTimer = new Timer(Random.Range(0.5f, 2f));
        }

        private bool IsTargetInFireRange() {
            return (ai.targetPosition - ai.owner.position).CompareLength(stats.fireDistance) < 0;
        }

        public override void Leave() {
            ai.alwaysLookAtTarget = false;
            ai.weapon.isTriggerPressed = false;
            ai.navAgent.speed /= boost;
            ai.navAgent.acceleration /= boost;
        }

        public override string ToString() {
            return base.ToString() + $"\nWaiting: {waiting}\nWaitTimer: {waitTimer}";
        }
    }
}
