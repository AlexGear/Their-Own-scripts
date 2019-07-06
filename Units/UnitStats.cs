using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    [System.Serializable]
    public class UnitStats {
        public float visionRange;
        public float fovAngle;
        public CurveAsset visionCheckInterval;
        public LayerMask navHindrances;
        public LayerMask attackTargets;
        public LayerMask visionObstacles;
        public bool dontRotateSearchingTarget;
    }
}