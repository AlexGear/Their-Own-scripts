using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public partial class BezierPath : MonoBehaviour {
        public class Slider {
            [System.Serializable]
            public struct SavableState {
                private readonly int index;
                private readonly float t;

                public SavableState(Slider slider) {
                    this.index = slider.segment.indexInPath;
                    this.t = slider.t;
                }

                public void ApplyTo(Slider slider) {
                    slider.segment = slider.path.GetSegmentAt(this.index);
                    slider.t = this.t;
                    slider.position = slider.segment.GetPosition(this.t);
                }
            }

            public BezierPath path { get; private set; }
            public Segment segment { get; private set; }

            private float _t;
            public float t {
                get => _t;
                set => _t = Mathf.Clamp01(value);
            }

            public Vector2 position { get; private set; }

            public Slider(BezierPath path) : this(path.segments[0], 0) {
            }

            public Slider(Segment segment, float t) {
                this.segment = segment;
                this.path = segment.path;
                this.t = t;
                this.position = segment.GetPosition(t);
            }

            public void MoveWithVelocity(float velocity) {
                if(segment == null)
                    return;

                _t += velocity / segment.GetVelocity(_t).magnitude * Time.deltaTime;
                position = segment.GetPosition(Mathf.Clamp01(_t));
                WrapT();
            }

            public SavableState GetState() => new SavableState(this);

            public void RestoreState(SavableState state) => state.ApplyTo(this);

            private void WrapT() {
                if(_t > 1) {
                    if(segment.next != null) {
                        segment = segment.next;
                        _t = 0;
                    }
                    else _t = 1;
                }
                else if(_t < 0) {
                    if(segment.prev != null) {
                        segment = segment.prev;
                        _t = 1;
                    }
                    else _t = 0;
                }
                else return;

                if(segment != null) {
                    position = segment.GetPosition(_t);
                }
            }

            public void MoveToCircumference(Vector2 center, float radius) {
                if(segment == null)
                    return;

                const float maxDelta = 0.2f;
                for(int i = 0; i < Mathf.CeilToInt(1 / maxDelta); i++) {
                    Vector3 z = segment.GetPosition(_t) - center;
                    Vector3 v = segment.GetVelocity(_t);
                    float m = z.magnitude;
                    float delta = m * (m - radius) / Vector3.Dot(v, z);
                    _t = _t - Mathf.Clamp(delta, -maxDelta, maxDelta);
                }
                position = segment.GetPosition(Mathf.Clamp01(_t));
                WrapT();
            }
        }
    }
}