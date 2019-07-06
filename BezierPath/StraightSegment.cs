using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public class StraightSegment : Segment {
        public Vector2 start, end;

        public StraightSegment(BezierPath path, Vector2 start, Vector2 end) : base(path) {
            this.start = start;
            this.end = end;
        }

        public override Vector2 GetPosition(float t) {
            return Vector2.LerpUnclamped(start, end, t);
        }

        public override Vector2 GetVelocity(float t) {
            return end - start;
        }

        public override void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);
        }
    }
}