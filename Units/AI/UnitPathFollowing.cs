using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public class UnitPathFollowing {
        private FollowPath _path;
        public FollowPath path {
            get { return _path; }
            set {
                if(_path == value)
                    return;
                _path = value;
                UpdatePathFollowing();
            }
        }

        private FollowPath _secondaryPath;
        public FollowPath secondaryPath {
            get { return _secondaryPath; }
            set {
                if(_secondaryPath == value)
                    return;
                _secondaryPath = value;
                UpdatePathFollowing();
            }
        }

        private const float updateInterval = 0.5f;
        private UnitAI ai;
        private Timer updateTimer;
        private Vector2 lastDestination;

        public UnitPathFollowing(UnitAI ai) {
            this.ai = ai;
            updateTimer = new Timer(updateInterval);
            updateTimer.SetOnEdge();
        }

        public void Think() {
            if(ai.IsInCombat(true)) {
                updateTimer.SetOnEdge();
                return;
            }
            if(updateTimer.Tick()) {
                UpdatePathFollowing();
            }
        }

        private void UpdatePathFollowing() {
            if(path == null || !path.isValid) return;

            const float pointPassRadius = 5;

            if(path.IsLastPointPassed(ai.owner.position, pointPassRadius)) {
                if(secondaryPath != null && secondaryPath.isValid) {
                    SwitchToSecondaryPath();
                }
                else return;
            }
            Vector2 nextPoint = path.GetNextPoint(ai.owner.position, pointPassRadius);

            const float maxOffset = 3.5f;
            Vector2 moveTo = nextPoint + Random.insideUnitCircle * maxOffset;
            Vector2 traversablePosition;
            if(ai.navAgent.SamplePosition(moveTo, maxOffset, out traversablePosition))
                moveTo = traversablePosition;
            else
                moveTo = nextPoint;

            ai.state = new MoveToPositionState(ai, moveTo);

            updateTimer.interval = Vector2.Distance(ai.owner.position, moveTo) / ai.navAgent.speed * 0.9f;
        }

        private void SwitchToSecondaryPath() {
            _path = _secondaryPath;
            _secondaryPath = null;
        }
    }
}