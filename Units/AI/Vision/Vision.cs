using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class Vision {
        public readonly SightedUnitAI ai;

        private float _lastFovAngle = 0;
        private float _fovHalfCos = 1; // cos(0)
        public float fovHalfCos {
            get {
                if(_lastFovAngle != ai.stats.fovAngle) {
                    _lastFovAngle = ai.stats.fovAngle;
                    _fovHalfCos = Mathf.Cos(Mathf.Deg2Rad * _lastFovAngle / 2);
                }
                return _fovHalfCos;
            }
        }

        public event System.Action<Unit> FoundClosestUnit;

        private Timer timer;
        private Dictionary<Unit, UnitNoticedFact> unitNoticedFacts;

        //private static Collider2D[] overlapResults = new Collider2D[100];


        public Vision(SightedUnitAI ai) {
            this.ai = ai;
            timer = new Timer(Random.Range(0, GetCheckInterval()));
            unitNoticedFacts = new Dictionary<Unit, UnitNoticedFact>();
        }

        // from specified point; ignoring fov
        public virtual bool WouldSeeUnitIgnoringFOVFrom(Vector2 position, Unit unit, bool straightLine = false) {
            if(!unit.Is()) {
                return false;
            }
            var toUnit = position - (Vector2)unit.transform.position;
            if(toUnit.CompareLength(ai.stats.visionRange) > 0) {
                return false;
            }
            return CachedVisibilityChecker.IsUnitVisibleFrom(unit, position, ai.stats.visionObstacles, straightLine);
        }

        // from owner's position; ignoring fov
        public bool WouldSeeUnitIgnoringFOV(Unit unit, bool straightLine = false) {
            return WouldSeeUnitIgnoringFOVFrom(ai.owner.position, unit, straightLine);
        }

        // from specified point; within fov
        public bool WouldSeeUnitFrom(Vector2 position, Unit unit, bool straightLine = false) {
            return IsUnitInFOV(unit) && WouldSeeUnitIgnoringFOVFrom(position, unit, straightLine);
        }

        // from owner's position; within fov
        public bool IsUnitSeen(Unit unit, bool straightLine = false) {
            return IsUnitInFOV(unit) && WouldSeeUnitIgnoringFOV(unit, straightLine);
        }

        public bool WasUnitSeenOnLastCheck(Unit unit) {
            return unitNoticedFacts.ContainsKey(unit);
        }

        public bool IsUnitInFOV(Unit unit) {
            if(!unit.Is()) {
                return false;
            }
            Vector2 vec = ((Vector2)unit.transform.position - ai.owner.position).normalized;
            return Vector2.Dot(vec, ai.owner.forward) > fovHalfCos;
        }

        public bool IsPointSeen(Vector2 point) {
            return WouldSeeUnitFrom(point, ai.owner, true);
        }
        
        private float GetCheckInterval() {
            if(ai.stats.visionCheckInterval != null) {
                float distance = Vector2.Distance(ai.owner.position, MainCharacter.current.position);
                return ai.stats.visionCheckInterval.Evaluate(distance);
            }
            else {
                if(!ai.IsInCombat(true)) {
                    return Random.Range(0.7f, 0.9f);
                }
                return Random.Range(0.3f, 0.4f);
            }
        }

        public void Think() {
            if(timer.Tick()) {
                timer.interval = GetCheckInterval();
                UpdateVision();
            }
        }

        private void UpdateVision() {
            GetAllVisibleUnitsFacts();
            if(ai is CommandedUnitAI commandedAI) {
                commandedAI.commander.UpdateVision(unitNoticedFacts);
            }
            Unit closest = unitNoticedFacts.Values.ClosestTo(ai.owner.position, f => f.position)?.unit;
            if(closest != null) {
                FoundClosestUnit?.Invoke(closest);
            }
        }

        private void GetAllVisibleUnitsFacts() {
            unitNoticedFacts.Clear();
            foreach(var unit in Unit.GetInRadius<Unit>(ai.owner.position, ai.stats.visionRange, ai.stats.attackTargets)) {
                if(IsUnitSeen(unit)) {
                    unitNoticedFacts[unit] = new UnitNoticedFact(unit);
                }
            }
        }

        public override string ToString() {
            return timer.ToString();
        }
    }
}