using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public partial class BezierPath : MonoBehaviour {
        [SerializeField] FollowPath path;

        private Segment[] segments;

        public int IndexOfSegment(Segment segment) => Array.IndexOf(segments, segment);

        public Segment GetSegmentAt(int index) => segments[index];

        private void Awake() {
            segments = CreateSegments();
        }

        private Segment[] CreateSegments() {
            int n = path.waypoints.Count;
            if(n < 2) {
                throw new Exception($"Cannot create bezier path from {n} points. There must be at least 2 points");
            }

            var segments = new Segment[n];

            segments[0]     = new StraightSegment(this, GetPoint(0), GetMiddlePoint(0));
            segments[n - 1] = new StraightSegment(this, GetMiddlePoint(n - 2), GetPoint(n - 1));
            for(int i = 1; i < n - 1; i++) {
                segments[i] = new BezierSegment(this, GetMiddlePoint(i - 1), GetPoint(i), GetMiddlePoint(i));
            }
            segments[0].prev = null;
            segments[0].next = segments[1];
            for(int i =  1; i < n - 1; i++) {
                segments[i].prev = segments[i - 1];
                segments[i].next = segments[i + 1];
            }
            segments[n - 1].prev = segments[n - 2];
            segments[n - 1].next = null;

            return segments;
        }

        private Vector2 GetPoint(int index) {
            if(index < 0 || index > path.waypoints.Count - 1) {
                throw new IndexOutOfRangeException($"Point index {index} is out of range [0; {path.waypoints.Count - 1}]");
            }
            return path.waypoints[index];
        }
        
        private Vector2 GetMiddlePoint(int index) {
            if(index < 0 || index > path.waypoints.Count - 2) {
                throw new IndexOutOfRangeException($"Middle point index {index} is out of range [0; {path.waypoints.Count - 2}]");
            }
            return (path.waypoints[index] + path.waypoints[index + 1]) * 0.5f;
        }

        private void OnDrawGizmos() {
            if(path == null) {
                return;
            }
            if(!Application.isPlaying && path.waypoints.Count >= 2) {
                segments = CreateSegments();
            }
            if(segments == null) {
                return;
            }
            foreach(var segment in segments) {
                segment.OnDrawGizmos();
            }
        }
    }
}