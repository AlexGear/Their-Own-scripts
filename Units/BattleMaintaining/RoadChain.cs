using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [System.Serializable]
    public abstract class RoadChain : ISavable {
        [SerializeField, HideInInspector] protected Cell[] cells;
        [SerializeField, HideInInspector] protected FollowPath path;

        [SerializeField, HideInInspector] protected float cellsRadius;
        [SerializeField, HideInInspector] protected float cellsInterval;

        public Cell firstCell => cells[0];

        public RoadChain(FollowPath path, float cellsRadius, float cellsInterval) {
            this.path = path;
            this.cellsRadius = cellsRadius;
            this.cellsInterval = cellsInterval;
        }

        protected abstract Cell[] GenerateCellsArray();

        public void GenerateCells() {
            cells = GenerateCellsArray();
        }

        public void OnStart() {
            foreach(var cell in cells) {
                cell.teamChanged += OnCellTeamChanged;
            }
        }

        private void OnCellTeamChanged(Cell cell) {
            foreach(var tryCaptureCell in cell.GetBackwardCells()) {
                if(tryCaptureCell.team == cell.team) continue;

                if(!tryCaptureCell.HasProtection(2)) {
                    tryCaptureCell.team = cell.team;
                }
            }
        }

        protected TCell CreateCellAt<TCell>(Vector2 position, int i) where TCell : Cell {
            GameObject go = new GameObject(typeof(TCell).Name + " " + i);
            go.transform.parent = BattleMaintainer.instance.cellsParent;
            go.transform.position = position;
            TCell cell = go.AddComponent<TCell>();
            cell.Generate(path, cellsRadius);
            return cell;
        }

        public virtual void ConnectCells(RoadManager roadManager) {
            for(int i = 0; i < cells.Length - 1; i++) {
                Cell.Connect(cells[i], cells[i + 1]);
            }
        }

        public Cell FindCellAtPoint(Vector2 point) {
            foreach(var cell in cells) {
                if(cell.IsPointInside(point))
                    return cell;
            }
            return null;
        }

        public Cell GetClosestCell(Vector2 point) {
            Cell result = null;
            float minSqrDist = float.PositiveInfinity;
            foreach(var cell in cells) {
                float sqrDist = (cell.position - point).sqrMagnitude;
                if(sqrDist < minSqrDist) {
                    minSqrDist = sqrDist;
                    result = cell;
                }
            }
            return result;
        }

        public void Destroy() {
            if(cells == null) return;

            foreach(var cell in cells) {
                if(cell != null)
                    Object.DestroyImmediate(cell.gameObject);
            }
        }

        public object CaptureSnapshot() {
            object[] cellSnapshots = new object[cells.Length];
            for(int i = 0; i < cellSnapshots.Length; i++) {
                cellSnapshots[i] = cells[i].CaptureSnapshot();
            }
            return cellSnapshots;
        }

        public void RestoreSnapshot(object data) {
            object[] cellSnapshots = (object[])data;
            for(int i = 0; i < cellSnapshots.Length; i++) {
                cells[i].RestoreSnapshot(cellSnapshots[i]);
            }
        }
    }
}