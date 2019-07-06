using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path : IPath {
    [SerializeField]
    private List<Vector2> _waypoints = new List<Vector2>();

    public List<Vector2> waypoints {
        get => _waypoints;
        set => _waypoints = value;
    }

    public bool isValid => waypoints != null && waypoints.Count >= 2;

    public Vector2 start => waypoints[0];
    public Vector2 end => waypoints[waypoints.Count - 1];

    public Path() {
    }
    
    public Path(IEnumerable<Vector2> waypoints) {
        this.waypoints = new List<Vector2>(waypoints);
    }

    public Vector2 this[int index] {
        get => waypoints[index];
        set => waypoints[index] = value;
    }

    public Vector2 GetWaypointByIndexClamped(int index) {
        if(waypoints.Count == 0)
            throw new System.Exception("Trying to get a waypoint from an empty path!");
        index = Mathf.Clamp(index, 0, waypoints.Count - 1);
        return waypoints[index];
    }

    public Vector2 GetPointAlongPathClamped(float distance) {
        if(waypoints.Count == 0)
            throw new System.Exception("Trying to get a point from an empty path!");

        if(distance <= 0)
            return waypoints[0];
        for(int i = 0; i < waypoints.Count - 1; i++) {
            Vector2 vector = waypoints[i + 1] - waypoints[i];
            float length = vector.magnitude;
            if(distance < length) {
                return waypoints[i] + vector / length * distance;
            }
            distance -= length;
        }
        return end;
    }

    public Vector2 GetDirectionAtPointAlongPathClamped(float distance) {
        if(waypoints.Count == 0)
            throw new System.Exception("Trying to get the direction at a point from an empty path!");

        if(waypoints.Count == 1)
            return Vector2.zero;

        if(distance < 0)
            return (waypoints[1] - waypoints[0]).normalized;

        for(int i = 0; i < waypoints.Count - 1; i++) {
            Vector2 vector = waypoints[i + 1] - waypoints[i];
            float length = vector.magnitude;
            if(distance < length) {
                return vector / length;
            }
            distance -= length;
        }
        return (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
    }

    public float GetTotalLength() {
        float result = 0;
        for(int i = 0; i < waypoints.Count - 1; i++) {
            result += Vector2.Distance(waypoints[i], waypoints[i + 1]);
        }
        return result;
    }

    public Vector2 GetClosestPointOnPath(Vector2 point) {
        Vector2 closestPoint;
        GetClosestPointOnPath(point, out closestPoint);
        return closestPoint;
    }

    /// <summary>
    /// Returns index of the start waypoint of the segment
    /// </summary>
    public int GetClosestPointOnPath(Vector2 point, out Vector2 closestPoint) {
        closestPoint = Vector2.positiveInfinity;
        int index = -1;
        for(int i = 0; i < waypoints.Count - 1; i++) {
            Ray segmentRay = GetSegmentRay(i);
            Vector2 toSegmentStart = point - waypoints[i];
            Vector2 toSegmentEnd = point - waypoints[i + 1];

            if(Vector2.Dot(toSegmentStart, segmentRay.direction) < 0 || // when segmentRay.GetClosestPoint result
               Vector2.Dot(toSegmentEnd, -segmentRay.direction) < 0)  //  is outside of the actual segment
            {
                if(waypoints[i].CloserToPointThan(point, closestPoint)) {
                    closestPoint = waypoints[i];
                    index = i;
                }
                if(waypoints[i + 1].CloserToPointThan(point, closestPoint)) {
                    closestPoint = waypoints[i + 1];
                    index = i + 1;
                }

                continue;
            }

            Vector2 x = segmentRay.GetClosestPoint(point);

            if(x.CloserToPointThan(point, closestPoint)) {
                closestPoint = x;
                index = i;
            }
        }
        return index;
    }

    private Ray GetSegmentRay(int index) {
        if(index < 0 || index >= waypoints.Count - 1)
            throw new System.ArgumentOutOfRangeException(nameof(index));

        return new Ray(waypoints[index], waypoints[index + 1] - waypoints[index]);
    }
}
