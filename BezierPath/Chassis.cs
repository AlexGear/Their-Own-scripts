using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public class Chassis : ISavable {
        [System.Serializable]
        private struct Snapshot {
            public BezierPath.Slider.SavableState frontSliderState;
            public BezierPath.Slider.SavableState rearSliderState;
        }

        public readonly BezierPath frontSliderPath;
        public readonly BezierPath rearSliderPath;
        public readonly float length;

        private readonly BezierPath.Slider frontSlider;
        private readonly BezierPath.Slider rearSlider;

        public Vector2 frontPosition => frontSlider.position;
        public Vector2 rearPosition => rearSlider.position;

        /// <summary>
        /// Equals to (<see cref="frontPosition"/> - <see cref="rearPosition"/>) (i. e. non-normalized)
        /// </summary>
        public Vector2 direction => frontPosition - rearPosition;

        public Quaternion rotation => Quaternion.LookRotation(Vector3.forward, direction);


        public Chassis(BezierPath frontSliderPath, BezierPath rearSliderPath, float length) {
            if(length <= 0) {
                throw new System.ArgumentOutOfRangeException(nameof(length));
            }
            this.frontSliderPath = frontSliderPath;
            this.rearSliderPath = rearSliderPath;
            this.length = length;

            frontSlider = new BezierPath.Slider(frontSliderPath);
            rearSlider = new BezierPath.Slider(rearSliderPath);

            PlaceOnStart();
        }

        public void PlaceOnStart() {
            // If the sliders are positioned at the same point
            // then both paths seem to be the same
            // and we should advance the front slider a little bit forward
            // to help it fit in place properly
            if(rearSlider.position == frontSlider.position) {
                frontSlider.t = 0.05f;
                frontSlider.MoveToCircumference(rearSlider.position, this.length);
            }
            else {
                rearSlider.MoveToCircumference(frontSlider.position, this.length);
                frontSlider.MoveToCircumference(rearSlider.position, this.length);
            }
        }

        public void MoveWithVelocity(float velocity) {
            float maxDistance = velocity * Time.deltaTime;

            var frontState = frontSlider.GetState();
            var rearState = rearSlider.GetState();

            var oldRearPos = rearSlider.position;

            frontSlider.MoveWithVelocity(velocity);
            rearSlider.MoveToCircumference(frontSlider.position, length);

            // If the rear slider moved too fast
            if((rearSlider.position - oldRearPos).CompareLength(maxDistance) > 0) {
                frontSlider.RestoreState(frontState);
                rearSlider.RestoreState(rearState);

                rearSlider.MoveWithVelocity(velocity);
                frontSlider.MoveToCircumference(rearSlider.position, length);
            }
        }

        public object CaptureSnapshot() => new Snapshot {
            frontSliderState = frontSlider.GetState(),
            rearSliderState = rearSlider.GetState()
        };

        public void RestoreSnapshot(object data) {
            var snapshot = (Snapshot)data;
            frontSlider.RestoreState(snapshot.frontSliderState);
            rearSlider.RestoreState(snapshot.rearSliderState);
        }
    }
}