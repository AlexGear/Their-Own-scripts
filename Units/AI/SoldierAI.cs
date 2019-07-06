using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AI {
    public class SoldierAI : UnitAIWithTarget {
        public override bool isMelee => false;

        private bool _alwaysLookAtTarget;
        public bool alwaysLookAtTarget {
            get { return _alwaysLookAtTarget; }
            set {
                _alwaysLookAtTarget = value;
                navAgent.updateRotation = !value;
            }
        }

        public override Unit target {
            set {
                base.target = value;
                if(value == null) {
                    alwaysLookAtTarget = false;
                }
            }
        }

        public override AIState state {
            set {
                if(state != value) {
                    weapon.isTriggerPressed = false;    // kostyl' lol
                    alwaysLookAtTarget = false;
                }
                if(value is SoldierHide && ((SoldierStats)stats).disallowHiding) {
                    base.state = new SoldierRetreat(this);
                    return;
                }
                base.state = value;
            }
        }

        public BaseWeapon weapon { get; private set; }

        private readonly float bulletSpeed;

        private Vector2? _damageSourcePos;
        private Vector2? damageSourcePos {
            get => _damageSourcePos;
            set {
                _damageSourcePos = value;
                lookAtDamageSourceTimer = value != null ? new Timer(Random.Range(3, 5)) : null;
            }
        }
        private Timer lookAtDamageSourceTimer;

        public SoldierAI(Unit unit, UnityEngine.AI.NavMeshAgent navAgent, UnitStats stats, BaseWeapon weapon)
            : base(unit, navAgent, stats) 
        {
            this.weapon = weapon;
            if(weapon != null && weapon is CommonWeapon commonWeapon) {
                bulletSpeed = commonWeapon.bulletSpeed;
            }
            state = new TargetSearchState(this);
        }

        public override void Think() {
            base.Think();
            alwaysLookAtTarget = isTargetVisible;
            if(alwaysLookAtTarget) {
                LookAt(targetPosition);
            }
            else if(damageSourcePos.HasValue) {
                if(!lookAtDamageSourceTimer.Tick()) {
                    LookAt(damageSourcePos.Value);
                }
                else {
                    damageSourcePos = null;
                }
            }
            else {
                var otherUnitNoticedFact = commander.GetClosestNoticedFact(owner.position);
                if(otherUnitNoticedFact != null) {
                    LookAt(otherUnitNoticedFact.position);
                }
            }
        }

        public override void OnDamageTaken(float value, Unit source) {
            if(owner.health < owner.maxHealth * 0.7f) {
                state = new SoldierHide(this, source);
                return;
            }
            else if(state is SoldierAttacking attackingState) {
                attackingState.Dodging();
            }

            if(source == null) {
                return;
            }
            damageSourcePos = source.position;
            if(vision.WouldSeeUnitIgnoringFOV(source)) {
                target = source;
                return;
            }
            state = new TargetSearchWanderingState(this, source.position, false);
        }

        private void LookAt(Vector2 lookTarget) {
            Vector2 lookDirection = lookTarget - owner.position;
            if(state is SoldierAttacking && bulletSpeed != 0) {
                lookDirection = Deflection.CalculateDirection(owner.position, lookTarget, bulletSpeed, targetVelocityTrend);
            }
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lookDirection);
            if(Mathf.Abs(Quaternion.Dot(rotation, owner.rotation)) > 0.999f)
                owner.rotation = rotation;
            else
                owner.rotation = Quaternion.Lerp(owner.rotation, rotation, Time.deltaTime * 10f);
        }

        public override void ContinueTargetPursuing() {
            if(target.Is()) {
                if(!(state is SoldierPursuingTarget)) {
                    state = new SoldierPursuingTarget(this);
                }
            }
            else {
                state = new TargetSearchState(this);
            }
        }

        public override void AttractAttention(Vector2 position) {
            if(!IsInCombat()) {
                state = new TargetSearchWanderingState(this, position, false);
            }
        }

        public override string ToString() {
            return $"- Always Look At Target: {alwaysLookAtTarget}\n- Trigger Pressed: {weapon.isTriggerPressed}\n" + base.ToString();
        }

#if UNITY_EDITOR
        public override void DrawGizmos() {
            base.DrawGizmos();
            Handles.color = Color.red;
            var ltp = lastTargetPosition;
            Handles.DrawLine(ltp + new Vector2(-1f, 0), ltp + new Vector2(1f, 0));
            Handles.DrawLine(ltp + new Vector2(0, -1f), ltp + new Vector2(0, 1f));
            Handles.DrawWireDisc(ltp, Vector3.forward, 1.5f);
        }
#endif
    }
}
