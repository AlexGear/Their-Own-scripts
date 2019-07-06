using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMaintaining {
    [System.Serializable]
    public class MainRoadChain : RoadChain {
        [SerializeField, HideInInspector] int alliesPersistentCellsNumber;

        public MainRoadChain(FollowPath path, float cellsRadius, float cellsInterval, int alliesPersistentCellsNumber)
            : base(path, cellsRadius, cellsInterval) 
        {
            this.alliesPersistentCellsNumber = alliesPersistentCellsNumber;
        }

        protected override Cell[] GenerateCellsArray() {
            int i = 0;
            float totalLength = path.GetTotalLength();
            var cellsList = new List<Cell>();
            for(float d = 0; d < totalLength; d += cellsInterval) {
                Vector2 cellPosition = path.GetPointAlongPathClamped(d);

                if(i++ < alliesPersistentCellsNumber)
                    cellsList.Add(CreateCellAt<AlliesPersistentCell>(cellPosition, i));
                else 
                    cellsList.Add(CreateCellAt<CapturableCell>(cellPosition, i));
            }
            return cellsList.ToArray();
        }
    }
}