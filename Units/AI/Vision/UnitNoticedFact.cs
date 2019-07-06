using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class UnitNoticedFact {
        /// <summary>
        /// The unit that was noticed
        /// </summary>
        public readonly Unit unit;
        
        /// <summary>
        /// The timestamp when the <see cref="unit"/> was noticed
        /// </summary>
        public readonly float time;
        
        /// <summary>
        /// The position where the <see cref="unit"/> stood when it was noticed
        /// </summary>
        public readonly Vector2 position;

        /// <summary>
        /// The velocity that the <see cref="unit"/> had when it was noticed
        /// </summary>
        public readonly Vector2 velocity;

        public UnitNoticedFact(Unit unit) {
            this.unit = unit;

            time = Time.time;
            position = unit.position;
            velocity = (unit as MovingUnit)?.velocityTrend ?? Vector2.zero;
        }
    }
}