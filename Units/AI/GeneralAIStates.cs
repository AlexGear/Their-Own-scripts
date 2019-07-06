using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AI {
    public class TargetSearchState : IntervalThinkState {
        protected UnitAIWithTarget ai;
        private float destAngle = float.PositiveInfinity;
        private float smoothDampVelocity;
        private bool firstIntervalThink = true;

        public TargetSearchState(UnitAIWithTarget ai) : base(interval: 1f) {
            this.ai = ai;
        }

        public TargetSearchState(UnitAIWithTarget ai, Quaternion lookAt) : this(ai) {
            destAngle = lookAt.eulerAngles.z;
        }

        public override void Enter() {
            if(float.IsInfinity(destAngle))
                destAngle = ai.owner.rotation.eulerAngles.z;

            if(ai.TryRequestTargetFromCommander()) {
                return;
            }
        }

        private void MoveAwayInRandomDirection(float distance) {
            Vector2 shift = Random.insideUnitCircle * distance;
            Vector2 moveTo = ai.owner.position + shift;

            NavMeshPath path;
            if(ai.navAgent.CanReach(moveTo, out path)) {
                ai.navAgent.SetPath2D(path);
                return;
            }

            NavMeshHit hit;
            if(ai.navAgent.Raycast2D(moveTo, out hit)) {
                moveTo = hit.position;
            }
            ai.navAgent.SetDestination2D(moveTo);
        }

        public override void Think() {
            base.Think();

            if(ai.stats.dontRotateSearchingTarget)
                return;

            float angle = ai.owner.rotation.eulerAngles.z;
            angle = Mathf.SmoothDampAngle(angle, destAngle, ref smoothDampVelocity, 0.57f);
            ai.owner.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public override void IntervalThink() {
            if(firstIntervalThink) {
                firstIntervalThink = false;
                MoveAwayInRandomDirection(20f);
            }

            if(ai.stats.dontRotateSearchingTarget)
                return;
            if(Random.value < 0.3f) {
                destAngle = Random.Range(-180f, 180f);
            }
        }

        public override void DrawGizmos() {
#if UNITY_EDITOR
            Color color = Color.yellow;
            Gizmos.color = color;
            Gizmos.DrawRay(ai.owner.position, ai.owner.forward * 2);
            color.a = 0.1f;
            Handles.color = color;
            Handles.DrawSolidArc(ai.owner.position, Vector3.forward, ai.owner.forward, Mathf.DeltaAngle(ai.owner.rotation.eulerAngles.z, destAngle), 2);
#endif
        }
    }

    public class TargetSearchWanderingState : IntervalThinkState {
        private bool isTurningAround = false;
        private Timer wanderingTimer;
        private float wanderingAreaRadius = 25f;
        private bool runAlongTargetVelocity;
        //private Timer remainingDistanceIsInfTimer;
        private UnitAIWithTarget ai;

        private const float searchPointReachDistance = 1.7f;

        public Vector2 searchPoint { get; private set; }
        public bool hasCheckedSearchPoint { get; private set; }

        public TargetSearchWanderingState(UnitAIWithTarget ai, Vector2 searchPoint, bool runAlongTargetVelocity) : base(0.25f) {
            this.ai = ai;
            this.searchPoint = searchPoint;
            this.runAlongTargetVelocity = runAlongTargetVelocity;
            this.hasCheckedSearchPoint = false;
        }

        public override void Enter() {
            wanderingTimer = new Timer(0);
            //remainingDistanceIsInfTimer = new Timer(2);
            ai.navAgent.SetDestination2D(searchPoint);
            if(ai is SoldierAI) {
                ((SoldierAI)ai).alwaysLookAtTarget = false;
            }
            Vector2 toSearchPoint = searchPoint - ai.owner.position;
            if(toSearchPoint.CompareLength(searchPointReachDistance) < 0) {
                OnSearchPointReached();
            }
        }

        public override void IntervalThink() {
            if(!hasCheckedSearchPoint) {
                RunToCheckingPoint();
                if(!hasCheckedSearchPoint && ai.vision.IsPointSeen(searchPoint)) {
                    ai.commander.ReportSearchPointIsClear(searchPoint, ai);
                }
            }
            else {
                Wandering();
            }
        }

        public void ReportedThatSearchPointIsClear() {
            runAlongTargetVelocity = false;
            hasCheckedSearchPoint = true;
            Wandering();
        }

        private void RunToCheckingPoint() {
            float remainingDistance = ai.navAgent.GetRemainingDistance();
            /*
            if(float.IsInfinity(remainingDistance))
                remDistInfTooLong = remainingDistanceIsInfTimer.Tick(thinkTimer.interval);
            else
                remainingDistanceIsInfTimer.time = 0;*/

            if(remainingDistance < 5f && ai is SoldierAI soldierAI) {
                soldierAI.alwaysLookAtTarget = true;
            }

            if(remainingDistance < searchPointReachDistance) {
                OnSearchPointReached();
            }
        }

        private void OnSearchPointReached() {
            if(ai is SoldierAI soldierAI) {
                soldierAI.alwaysLookAtTarget = false;
            }
            if(runAlongTargetVelocity) {
                StartOvertakingAttempt();
            }
            else {
                hasCheckedSearchPoint = true;
                ai.commander.ReportSearchPointIsClear(searchPoint, ai);
            }
        }

        public override void Leave() {
            if(ai is SoldierAI soldierAI) {
                soldierAI.alwaysLookAtTarget = false;
            }
        }

        private void StartOvertakingAttempt() {
            runAlongTargetVelocity = false;
            hasCheckedSearchPoint = false;
            float timeTookToGetHere = Time.time - ai.lastTargetSeenTime;
            searchPoint += ai.lastTargetVelocityTrend * timeTookToGetHere;
            ai.navAgent.SetDestination2D(searchPoint);
        }

        private void Wandering() {
            if(wanderingTimer.Tick()) {
                if(!isTurningAround) {
                    isTurningAround = true;
                    WanderingStartTurningAround();
                }
                else {
                    isTurningAround = false;
                    WanderingStartRepositioning();
                }
            }
        }

        private void WanderingStartTurningAround() {
            if(!ai.navAgent.pathPending)
                ai.navAgent.SetDestination2D(ai.owner.position - ai.owner.forward * ai.navAgent.radius);
            wanderingTimer.interval = Random.Range(0.3f, 0.7f);
        }

        private Vector3 FindRepositioningTargetAndGo() {
            NavMeshAgent navAgent = ai.navAgent;
            Vector2 moveTo;

            float radius = wanderingAreaRadius;
            for(int i = 0; i < 3; i++) {
                NavMeshPath path;
                moveTo = searchPoint + Random.insideUnitCircle * radius;
                if(navAgent.CanReach(moveTo, out path)) {
                    navAgent.SetPath2D(path);
                    return moveTo;
                }
                radius *= 0.5f;
            }

            moveTo = searchPoint + Random.insideUnitCircle * wanderingAreaRadius;
            NavMeshHit hit;
            if(navAgent.Raycast2D(moveTo, out hit)) {
                moveTo = hit.position;
            }
            if(!navAgent.pathPending)
                navAgent.SetDestination2D(moveTo);
            return moveTo;
        }

        private void WanderingStartRepositioning() {
            Vector2 moveTo = FindRepositioningTargetAndGo();
            wanderingTimer.interval = Vector2.Distance(moveTo, ai.owner.position) / ai.navAgent.speed;
        }

        private bool RanTooFar() {
            Vector2 toSearchPoint = searchPoint - (Vector2)ai.owner.position;
            float sqrAreaRadius = wanderingAreaRadius * wanderingAreaRadius;
            return toSearchPoint.sqrMagnitude > sqrAreaRadius;
        }

        public override string ToString() {
            //float remDist = ai.navAgent.remainingDistance;
            //string result = base.ToString() + $"\nWandering Timer: {wanderingTimer}\nHas Checked Search Point: {hasCheckedSearchPoint}\nRemaining Distance: {remDist}";
            string result = base.ToString() + $"\nWandering Timer: {wanderingTimer}\nHas Checked Search Point: {hasCheckedSearchPoint}";
            /*if(float.IsInfinity(remDist)) {
                result += $"\nRemain Dist Is Inf Time: {remainingDistanceIsInfTimer}";
            }*/
            return result;
        }
    }

    public class DamagedState : IntervalThinkState {
        private UnitAIWithTarget ai;
        private Vector2 hitNormal;

        public DamagedState(UnitAIWithTarget ai, Vector2 hitNormal)
            : base(Random.Range(0.2f, 0.4f)) {
            this.ai = ai;
            this.hitNormal = hitNormal;
        }

        public override void Think() {
            base.Think();
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, hitNormal);
            ai.owner.rotation = Quaternion.Lerp(ai.owner.rotation, rotation, Time.deltaTime * 10f);
        }

        public override void IntervalThink() {
            ai.state = new DamagedNewTargetSearchState(ai);
        }
    }

    public class DamagedNewTargetSearchState : TargetSearchState {
        private Unit prevTarget;

        public DamagedNewTargetSearchState(UnitAIWithTarget ai) : base(ai) { }

        public override void Enter() {
            base.Enter();
            prevTarget = ai.target;
            ai.target = null;
        }

        public override void IntervalThink() {
            base.IntervalThink();
            if(!ai.target.Is()) {
                ai.target = prevTarget;
            }
        }
    }

    public class MoveToPositionState : IntervalThinkState {
        private UnitAI ai;
        public Vector2 destination { get; private set; }
        private AIState nextState = null;

        public MoveToPositionState(UnitAI ai, Vector2 destination) : base(0.26f) 
        {
            this.ai = ai;
            this.destination = destination;
        }

        public MoveToPositionState(UnitAI ai, Vector2 destination, AIState nextState) 
            : this(ai, destination) 
        {
            this.nextState = nextState;
        }

        public override void IntervalThink() {
            if((Vector2)ai.navAgent.destination != destination) {
                ai.navAgent.SetDestination2D(destination);
            }
            if(ai.navAgent.ReachedDestination(0.2f)) {
                if(nextState != null) {
                    ai.state = nextState;
                    return;
                }
                if(ai is UnitAIWithTarget aiWithTarget) {
                    ai.state = new TargetSearchState(aiWithTarget);
                    return;
                }
            }
        }

        public override void Enter() {
            ai.navAgent.SetDestination2D(destination);
        }
    }

    public class FollowObjectState : AIState {
        private UnitAI ai;
        private Transform target;
        private float keepDistance;

        private Vector2 lastPosition;
        private Vector2 lastTargetPosition;
        private const float stationaryThreshold = 1.5f;

        public FollowObjectState(UnitAI ai, Transform target, float keepDistance = 1f) {
            this.ai = ai;
            this.target = target;
            this.keepDistance = keepDistance;
        }

        public override void Enter() {
            UpdateDestination();
            lastPosition = ai.owner.position;
            lastTargetPosition = target.position;
        }

        public override void Think() {
            Vector2 currentPosition = ai.owner.position;
            Vector2 currentTargetPosition = target.position;
            if((currentPosition - lastPosition).CompareLength(stationaryThreshold) > 0 ||
               (currentTargetPosition - lastTargetPosition).CompareLength(stationaryThreshold) > 0) 
            {
                UpdateDestination();
                lastPosition = currentPosition;
                lastTargetPosition = currentTargetPosition;
            }
        }

        private void UpdateDestination() {
            if(keepDistance == 0) {
                ai.navAgent.SetDestination2D(target.position);
                return;
            }
            var vector = (Vector2)target.position - ai.owner.position;
            var destination = (Vector2)target.position - vector.normalized * keepDistance;
            ai.navAgent.SetDestination2D(destination);
        }
    }
}
