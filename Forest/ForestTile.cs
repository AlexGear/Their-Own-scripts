using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;

namespace Forest {

    [CreateAssetMenu(menuName = "Tiles/ForestTile")]
    public class ForestTile : Tile {
#if UNITY_EDITOR
        public CandidateTable candidates;
#endif
    }

}