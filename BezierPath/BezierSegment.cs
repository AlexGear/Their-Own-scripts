using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public class BezierSegment : Segment {
        public readonly Vector2 p0, p1, p2;

        public BezierSegment(BezierPath path, Vector2 p0, Vector2 p1, Vector2 p2) : base(path) {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }

        public override Vector2 GetPosition(float t) {
            return t * ((p1 - p0) * (2 - t) + t * (p2 - p1)) + p0;
        }

        public override Vector2 GetVelocity(float t) {
            return 2 * (t * (p2 - 2 * p1 + p0) + p1 - p0);
        }

        public override void OnDrawGizmos() {
            Color color = Color.cyan;
            color.a = 0.3f;
            Gizmos.color = color;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);

            const float step = 0.03f;
            Vector2 prev = p0;
            Gizmos.color = Color.yellow;
            for(float x = step; x < 1; x += step) {
                Vector2 pos = GetPosition(x);
                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }
        }
    }
}
