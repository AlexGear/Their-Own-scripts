using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public abstract class UnitAIWithTarget : CommandedUnitAI {
        protected bool sameLayerTargetsAllowed = false;

        public Vector2 targetPosition {
            get {
                if(target.Is() && isTargetVisible)
                    return target.position;
                return lastTargetPosition;
            }
        }

        public Vector2 targetVelocityTrend {
            get {
                if(movingUnitTarget == null)
                    return Vector2.zero;
                return isTargetVisible ? (Vector2)movingUnitTarget.velocityTrend : lastTargetVelocityTrend;
            }
        }

        public float lastTargetSeenTime => commander.GetUnitNoticedFact(target)?.time ?? 0;
        public Vector2 lastTargetPosition => commander.GetUnitNoticedFact(target)?.position ?? owner.position;
        public Vector2 lastTargetVelocityTrend => commander.GetUnitNoticedFact(target)?.velocity ?? Vector2.zero;

        public bool isTargetVisible => target.Is() && vision.WasUnitSeenOnLastCheck(target);

        protected MovingUnit movingUnitTarget { get; private set; } = null;
        private Unit _target;
        public virtual Unit target {
            get { return _target; }
            set {
                if(value != null && (!value.Is() || _target.Is() && value.Is() && value == target && IsInCombat() || 
                    IsTeammateAndNotAllowedToBeTarget(value)))
                {
                    return;
                }

                if(_target)
                    _target.Died -= OnTargetDied;

                _target = value;
                movingUnitTarget = _target as MovingUnit;
                if(_target != null) {
                    _target.Died += OnTargetDied;
                    if(!(state is SoldierHide)) {
                        ContinueTargetPursuing();
                    }
                }
                else
                    TryRequestTargetFromCommander();
            }
        }

        public UnitAIWithTarget(Unit owner, NavMeshAgent navAgent, UnitStats stats)
            : base(owner, navAgent, stats) {
            vision.FoundClosestUnit += OnFoundClosestUnit;
        }

        public virtual void ContinueTargetPursuing() {
        }

        public override void Think() {
            if(!target.Is()) {
                target = null;
            }
            if(!isTargetVisible) {
                TryRequestTargetFromCommander();
            }
            base.Think();
        }

        public bool TryRequestTargetFromCommander() {
            Unit commanderTarget = commander.GetClosestNoticedFact(owner.position)?.unit;
            if(commanderTarget.Is()) {
                target = commanderTarget;
                return true;
            }
            return false;
        }

        public override string ToString() {
            return $"Target: {(target != null ? target.ToString() : "<NONE>")}\nIs Target Visible: { isTargetVisible}\n" + base.ToString();
        }

        private bool IsTeammateAndNotAllowedToBeTarget(Unit value) {
            return !sameLayerTargetsAllowed && value.gameObject.layer == owner.gameObject.layer;
        }

        private void OnFoundClosestUnit(Unit unit) {
            var cmdTarget = commander.GetClosestNoticedFact(owner.position)?.unit;
            if(cmdTarget.Is() && cmdTarget.position.CloserToPointThan(owner.position, unit.position)) {
                target = cmdTarget;
            }
            else {
                target = unit;
            }
        }

        private void OnTargetDied(Unit t) {
            this.target = null;
        }
    }
}