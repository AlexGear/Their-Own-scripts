using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public abstract class Segment {
        public readonly BezierPath path;
        public Segment prev, next;

        public int indexInPath => path != null ? path.IndexOfSegment(this) : -1;

        public Segment(BezierPath path) {
            this.path = path;
        }

        public abstract Vector2 GetPosition(float t);
        public abstract Vector2 GetVelocity(float t);
        public virtual void OnDrawGizmos() { }
    }
}
