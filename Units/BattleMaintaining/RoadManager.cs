using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [System.Serializable]
    public class RoadManager : ISavable {
        [SerializeField] FollowPath mainPath;
        [SerializeField] FollowPath[] secondaryPaths = new FollowPath[0];
        [SerializeField] float cellsRadius = 18;
        [SerializeField] float cellsInterval = 29;
        [SerializeField] int alliesPersistentCellsNumber = 3;

        [SerializeField, HideInInspector] MainRoadChain mainChain;
        [SerializeField, HideInInspector] SecondaryRoadChain[] secondaryChains;

        public float GetCellsRadius() => cellsRadius;
        public float GetCellsInterval() => cellsInterval;
        public FollowPath GetMainPath() => mainPath;
        
        public void GenerateChains() {
            TryToDestroyChains();
            mainChain = new MainRoadChain(mainPath, cellsRadius, cellsInterval, alliesPersistentCellsNumber);
            secondaryChains = new SecondaryRoadChain[secondaryPaths.Length];

            mainChain.GenerateCells();
            for(int i = 0; i < secondaryChains.Length; i++) {
                secondaryChains[i] = new SecondaryRoadChain(secondaryPaths[i], cellsRadius, cellsInterval);
                secondaryChains[i].GenerateCells();
            }
            mainChain.ConnectCells(this);
            for(int i = 0; i < secondaryChains.Length; i++) {
                secondaryChains[i].ConnectCells(this);
            }
        }

        public void OnStart() {
            if(mainChain == null || secondaryChains == null)
                throw new System.Exception("Chains were not generated!");

            mainChain.OnStart();
            foreach(var chain in secondaryChains) {
                chain.OnStart();
            }
        }

        public Cell FindCellAtPoint(Vector2 point) {
            Cell result = mainChain.FindCellAtPoint(point);
            if(result != null)
                return result;

            foreach(var chain in secondaryChains) {
                result = chain.FindCellAtPoint(point);
                if(result != null)
                    return result;
            }
            return null;
        }

        public Cell GetClosestCell(Vector2 point) {
            Cell result = mainChain.GetClosestCell(point);
            float minSqrDist = (result.position - point).sqrMagnitude;
            foreach(var chain in secondaryChains) {
                Cell cell = chain.GetClosestCell(point);
                float sqrDist = (cell.position - point).sqrMagnitude;
                if(sqrDist < minSqrDist) {
                    minSqrDist = sqrDist;
                    result = cell;
                }
            }
            return result;
        }

        public IEnumerable<ConflictLocation> FindAllConflictLocations() {
            var visited = new HashSet<Cell>();
            var toVisit = new HashSet<Cell> { mainChain.firstCell };
            while(true) {
                toVisit.ExceptWith(visited);
                if(toVisit.Count == 0)
                    break;

                var toVisitCopy = new HashSet<Cell>(toVisit);
                foreach(var cell in toVisitCopy) {
                    visited.Add(cell);
                    foreach(var o in cell.outcoming) {
                        toVisit.Add(o);
                        if(cell.dominantTeam != o.dominantTeam) {
                            yield return new ConflictLocation(cell, o);
                        }
                    }
                }
            }
        }

        private void TryToDestroyChains() {
            if(mainChain != null) mainChain.Destroy();
            if(secondaryChains != null) {
                foreach(var chain in secondaryChains) {
                    if(chain != null)
                        chain.Destroy();
                }
            }
        }

        public object CaptureSnapshot() {
            object[] snapshots = new object[1 + secondaryChains.Length];
            snapshots[0] = mainChain.CaptureSnapshot();
            for(int i = 0; i < secondaryChains.Length; i++) {
                snapshots[i + 1] = secondaryChains[i].CaptureSnapshot();
            }
            return snapshots;
        }

        public void RestoreSnapshot(object data) {
            object[] snapshots = (object[])data;
            mainChain.RestoreSnapshot(snapshots[0]);
            for(int i = 0; i < secondaryChains.Length; i++) {
                secondaryChains[i].RestoreSnapshot(snapshots[i + 1]);
            }
        }
    }
}