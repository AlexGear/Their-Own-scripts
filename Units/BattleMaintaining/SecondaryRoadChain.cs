using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [System.Serializable]
    public class SecondaryRoadChain : RoadChain {
        public SecondaryRoadChain(FollowPath path, float cellsRadius, float cellsInterval) 
            : base(path, cellsRadius, cellsInterval) { }

        protected override Cell[] GenerateCellsArray() {
            int i = 0;
            float totalLength = path.GetTotalLength();
            var cellsList = new List<Cell>();
            for(float d = cellsInterval; d < totalLength - cellsInterval; d += cellsInterval) {
                Vector2 cellPosition = path.GetPointAlongPathClamped(d);
                var newCell = CreateCellAt<CapturableCell>(cellPosition, i++);
                cellsList.Add(newCell);
            }
            return cellsList.ToArray();
        }

        public override void ConnectCells(RoadManager roadManager) {
            base.ConnectCells(roadManager);
            Cell startCell = roadManager.FindCellAtPoint(path.start);
            if(startCell == null) {
                throw new System.Exception("No chain cell was found at the path START. Secondary chain must start and end at other chains' cells");
            }
            Cell endCell = roadManager.FindCellAtPoint(path.end);
            if(endCell == null) {
                throw new System.Exception("No chain cell was found at the path END. Secondary chain must start and end at other chains' cells");
            }
            Cell.Connect(startCell, cells[0]);
            Cell.Connect(cells[cells.Length - 1], endCell);
        }
    }
}