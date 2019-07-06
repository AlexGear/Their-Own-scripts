using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    public class BattleMaintainer : MonoBehaviour, ISavable {
        [SerializeField] RoadManager roadManager;
        [SerializeField] UnitsManager unitsManager;
        [SerializeField] float maxDistanceFromMainCharacter = 70;
        [SerializeField] float interval = 5f;

        const int outOfSightSearchMaxDepth = 6;

        private Timer timer;
        private List<Unit> diedVitalAllies = new List<Unit>();
        private UnitsPool unitsPool;

        private static BattleMaintainer _instance;
        public static BattleMaintainer instance => _instance != null ? _instance : (_instance = FindObjectOfType<BattleMaintainer>());
        
        private Transform _cellsRoot;
        public Transform cellsParent {
            get {
                const string cellsParentName = "Road Cells Root";
                if(_cellsRoot != null)
                    return _cellsRoot;
                else if(_cellsRoot = transform.Find(cellsParentName)) {
                    return _cellsRoot;
                }
                var cellsRootObject = new GameObject(cellsParentName);
                cellsRootObject.transform.parent = this.transform;
                return (_cellsRoot = cellsRootObject.transform);
            }
        }

        [ContextMenu("Generate Chains")]
        private void GenerateChains() {
            roadManager.GenerateChains();
        }

        [ContextMenu("Find Conflicts")]
        private void FindConflicts() {
            foreach(ConflictLocation conflict in roadManager.FindAllConflictLocations()) {
                Vector2 a = conflict.alliesCell.position;
                Vector2 b = conflict.enemiesCell.position;
                Vector2 center = (a + b) * 0.5f;
                Vector2 perpendicular = Vector2.Perpendicular(a - b);
                Debug.DrawLine(center + perpendicular, center - perpendicular, Color.yellow, 5);
            }
        }

        private void Start() {
            unitsPool = new UnitsPool(this);
            roadManager.OnStart();
            unitsManager.OnStart();
            unitsManager.VitalAllyDied += OnVitalAllyDied;
            timer = new Timer(interval);
            timer.remaining = interval * 0.1f;
        }

        private void OnVitalAllyDied(Unit vitalAlly) {
            diedVitalAllies.Add(vitalAlly);
        }

        private void Update() {
            if(timer.Tick()) {
                Maintain();
            }
        }

        private void Maintain() {
            ReviveVitalUnits();
            foreach(var conflict in roadManager.FindAllConflictLocations()) {
                MaintainConflict(conflict);
            }
        }

        private void ReviveVitalUnits() {
            var toRemove = new List<Unit>();
            foreach(var diedVitalAlly in diedVitalAllies) {
                if(ReviveVitalUnitOutOfSight(diedVitalAlly)) {
                    toRemove.Add(diedVitalAlly);
                }
            }
            foreach(var u in toRemove) {
                diedVitalAllies.Remove(u);
            }
        }

        private void MaintainConflict(ConflictLocation conflictLocation) {
            var center = conflictLocation.center;

            if((center - MainCharacter.current.position).CompareLength(maxDistanceFromMainCharacter) > 0)
                return;

            var unitSet = unitsManager.GetUnitSetAt(center);
            if(unitSet == null) {
#if UNITY_EDITOR
                Debug.DrawRay(center, Vector3.up * 4, Color.red, 999);
                Debug.DrawRay(center, Vector3.down * 4, Color.red, 999);
                Debug.DrawRay(center, Vector3.right * 4, Color.red, 999);
                Debug.DrawRay(center, Vector3.left * 4, Color.red, 999);
#endif
                throw new System.Exception($"No UnitSet is found at point {center}");
            }
            float radius = roadManager.GetCellsInterval() * 3;
            var unitsToSpawn = unitSet.GetUnitsToSpawnAroundConflict(conflictLocation, radius);
            var unitInstances = unitsPool.SpawnUnits(unitsToSpawn, conflictLocation);
            foreach(var unitInstance in unitInstances) {
                SetupUnitAI(unitInstance, conflictLocation);
            }
        }

        private void SetupUnitAI(GameObject unitInstance, ConflictLocation conflictLocation) {
            var ai = unitInstance.GetComponent<Unit>()?.ai;
            if(ai != null) {
                Vector2 moveTo = conflictLocation.center + Random.insideUnitCircle * 15f;
                ai.navAgent.SamplePosition(moveTo, 9f, out moveTo);
                ai.state = new AI.MoveToPositionState(ai, moveTo);
                
                if(unitInstance.IsAlly()) {
                    ai.followPath = conflictLocation.enemiesCell.GetPath();
                    ai.secondaryFollowPath = roadManager.GetMainPath();
                }
            }
        }

        public bool FindPositionOutOfSight(Vector2 originalPos, Team team, out Vector2 result) {
            Cell cell = roadManager.GetClosestCell(originalPos);
            return FindPositionOutOfSight(cell, cell, team, out result);
        }

        public bool FindPositionOutOfSight(ConflictLocation conflict, Team team, out Vector2 result) {
            Cell thisCell = team == Team.Allies ? conflict.alliesCell : conflict.enemiesCell;
            Cell thatCell = team == Team.Allies ? conflict.enemiesCell : conflict.alliesCell;
            return FindPositionOutOfSight(thisCell, thatCell, team, out result);
        }

        public bool FindPositionOutOfSight(Cell thisCell, Cell thatCell, Team team, out Vector2 result) {
            Cell outOfSightCell = FindOutOfSightCell(thatCell, team, team);
            if(outOfSightCell != null) {
                result = outOfSightCell.position;
                return true;
            }
            if(team == Team.Allies) {   // searching an out of sight position on the opposite team's area is only for allies; don't allow enemies to spawn behind our back
                outOfSightCell = FindOutOfSightCell(thisCell, team, team.Opposite());
                if(outOfSightCell != null) {
                    result = outOfSightCell.position;
                    return true;
                }
            }
            result = default(Vector2);
            return false;
        }

        private bool ReviveVitalUnitOutOfSight(Unit unit) {
            Vector2 originalPosition = unit.position;
            Vector2 position;
            Team team;
            unit.gameObject.IsAllyOrEnemyObject(out team);
            if(FindPositionOutOfSight(unit.position, team, out position)) {
                unit.position = position;
                unit.Revive();
                if(unit.ai != null) {
                    unit.ai.state = new AI.MoveToPositionState(unit.ai, originalPosition);
                    unit.ai.followPath = roadManager.GetMainPath();
                }
                return true;
            }
            return false;
        }

        private Cell FindOutOfSightCell(Cell cell, Team team, Team searchDirectionTeam, int level = 1) {
            if(!CameraFollow.current.IsPointSeen(cell.position, new Vector2(9, 9)) &&
                NoSpawnArea.CanSpawnAtPoint(cell.position)) 
            {
                if(team == Team.Allies)
                    return cell;
                else if(!IsTherePlayerAhead(cell, level))
                    return cell;
            }   

            if(level >= outOfSightSearchMaxDepth)
                return null;

            foreach(var backCell in cell.GetBackwardCells(searchDirectionTeam)) {
                Cell outOfSightCell = FindOutOfSightCell(backCell, team, searchDirectionTeam, level + 1);
                if(outOfSightCell != null)
                    return outOfSightCell;
            }
            return null;
        }

        private bool IsTherePlayerAhead(Cell cell, int level) {
            if(cell.IsPointInside(MainCharacter.current.position, cell.radius * 2))
                return true;

            if(level >= outOfSightSearchMaxDepth)
                return false;

            foreach(var forwardCell in cell.GetForwardCells(Team.Allies)) {
                if(IsTherePlayerAhead(forwardCell, level + 1))
                    return true;
            }
            return false;
        }

        public object CaptureSnapshot() {
            return roadManager.CaptureSnapshot();
        }

        public void RestoreSnapshot(object data) {
            roadManager.RestoreSnapshot(data);
        }
    }
}
