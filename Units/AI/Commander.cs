using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI {
    public class Commander {
        public CommandedUnitAI ai { get; private set; }

        private HashSet<CommandedUnitAI> commandedAIs;

        private const float commanderSearchRadius = 50f;
        private static Collider2D[] overlapResults = new Collider2D[100];

        private Dictionary<Unit, UnitNoticedFact> unitNoticedFacts;
        private Timer factsCheckTimer = new Timer(1f);

        private const float densityCheckRadius = 5f;
        private const int maxUnitsOnDensityCheck = 4;
        private const float splitDistance = 10f;

        private Timer densityCheckTimer = new Timer(3f);

        public Commander(CommandedUnitAI ai) {
            this.ai = ai;
            commandedAIs = new HashSet<CommandedUnitAI>();
            unitNoticedFacts = new Dictionary<Unit, UnitNoticedFact>();
            AddCommandedAI(ai);
        }
        
        public static Commander FindCommanderFor(CommandedUnitAI ai) {
            int layerMask = 1 << ai.owner.gameObject.layer;
            var position = ai.owner.position;
            var units = Unit.GetInRadius<Unit>(position, commanderSearchRadius, layerMask);
            Unit closest = units.ClosestTo(position, u => u != ai.owner && u.ai is CommandedUnitAI cmd && cmd.isCommanderSelf);

            if(closest != null)
                return ((CommandedUnitAI)closest.ai).commander;

            return new Commander(ai);
        }

        public UnitNoticedFact GetUnitNoticedFact(Unit unit) {
            if(unit == null)
                return null;

            UnitNoticedFact fact;
            unitNoticedFacts.TryGetValue(unit, out fact);
            return fact;
        }

        public void AddCommandedAI(CommandedUnitAI ai) {
            if(commandedAIs.Contains(ai)) {
                return;
            }
            commandedAIs.Add(ai);
            
            ai.owner.Died += OnUnitDied;
        }

        public void RemoveCommandedAI(CommandedUnitAI ai) {
            if(!commandedAIs.Contains(ai)) {
                return;
            }
            commandedAIs.Remove(ai);
            ai.owner.Died -= OnUnitDied;
        }

        private void OnUnitDied(Unit unit) {
            RemoveCommandedAI(unit.ai as CommandedUnitAI);
            if(unit.ai == this.ai) {
                if(commandedAIs.Count == 0) {
                    return;
                }
                this.ai = commandedAIs.First();
            }
        }

        public void Think() {
            if(densityCheckTimer.Tick()) {
                CheckDensity();
            }
            if(factsCheckTimer.Tick()) {
                CheckFactsValidity();
            }
        }

        public void ReportSearchPointIsClear(Vector2 searchPoint, CommandedUnitAI reporter) {
            const float distanceTolerance = 2f;
            foreach(var commandedAI in commandedAIs) {
                if(commandedAI == reporter) {
                    continue;
                }
                if(commandedAI.state is TargetSearchWanderingState wanderingState &&
                    !wanderingState.hasCheckedSearchPoint &&
                    (wanderingState.searchPoint - searchPoint).CompareLength(distanceTolerance) < 0)
                {
                    wanderingState.ReportedThatSearchPointIsClear();
                }
            }
        }

        private void CheckFactsValidity() {
            var keysToRemove = new List<Unit>();
            foreach(var key in unitNoticedFacts.Keys) {
                if(!key.Is()) {
                    keysToRemove.Add(key);
                }
            }
            foreach(var key in keysToRemove) {
                unitNoticedFacts.Remove(key);
            }
        }

        private void CheckDensity() {
            var ignoreList = new List<CommandedUnitAI>() { this.ai };
            var overlapList = new List<CommandedUnitAI>();
            foreach(var unitToCheck in commandedAIs) {
                if(ignoreList.Contains(unitToCheck)) {
                    continue;
                }
                Vector2 center = unitToCheck.owner.position;
                overlapList.Clear();
                foreach(var commandedAI in commandedAIs) {
                    if(!commandedAI.isMelee && (commandedAI.owner.position - center).CompareLength(densityCheckRadius) < 0) {
                        overlapList.Add(commandedAI);
                    }
                }
                ignoreList.AddRange(overlapList);
                if(overlapList.Count > maxUnitsOnDensityCheck) {
                    SplitGroup(overlapList);
                }
            }
        }

        public UnitNoticedFact GetClosestNoticedFact(Vector2 position) {
            return unitNoticedFacts.Values.ClosestTo(position, f => f.unit.position, filter: f => f.unit.Is());
        }
        
        /*public Unit GetClosestTarget(Vector2 position) {
            //Vector2 position = requester.owner.position;
            var fact = GetClosest
            return fact?.unit;
        }*/

        private static Unit GetAnyTarget(IEnumerable<UnitAIWithTarget> among, out UnitAIWithTarget witness) {
            witness = among.FirstOrDefault(x => x.target.Is());
            return witness?.target;
        }

        private void SplitGroup(IEnumerable<UnitAI> group) {
            Vector2 groupPosition = group.First().owner.position;
            var target = GetAnyTarget(group.OfType<UnitAIWithTarget>(), out _);
            Vector2 splitNormal;
            if(target != null) {
                splitNormal = ((Vector2)target.transform.position - groupPosition).normalized;
            }
            else {
                splitNormal = Random.insideUnitCircle.normalized;
            }
            Vector2 destination1 = groupPosition + Vector2.Perpendicular(splitNormal) * splitDistance;
            Vector2 destination2 = groupPosition - Vector2.Perpendicular(splitNormal) * splitDistance;
            int n = group.Count();
            int i = 0;
            foreach(var groupMember in group) {
                var dest = i < n / 2 ? destination1 : destination2;
                groupMember.state = new MoveToPositionState(groupMember, destination1);
            }
        }

        public void UpdateVision(IDictionary<Unit, UnitNoticedFact> witnessUnitNoticedFacts) {
            foreach(var pair in witnessUnitNoticedFacts) {
                this.unitNoticedFacts[pair.Key] = pair.Value;
            }
            /*for(int i = 0; i < witnessUnitNoticedFacts.Count; i++) {
                UnitNoticedFact fact = witnessUnitNoticedFacts[i];
                this.unitNoticedFacts[fact.unit] = fact;
            }*/
        }

#if UNITY_EDITOR
        public void DrawGizmos() {
            UnityEditor.Handles.color = Color.magenta;
            var start = this.ai.owner.position;
            foreach(var commandedAI in commandedAIs)
                UnityEditor.Handles.DrawLine(start, commandedAI.owner.position);
        }
#endif
    }
}