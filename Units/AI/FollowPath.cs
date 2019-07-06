using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class FollowPath : MonoBehaviour, IPath {
#if UNITY_EDITOR
    [SerializeField] bool drawGizmos = true;
#endif

    [SerializeField]
    private Path path = new Path();

    public List<Vector2> waypoints {
        get => path.waypoints;
        set => path.waypoints = value;
    }

    public bool isValid => path.isValid;

    public Vector2 start => path.start;
    public Vector2 end => path.end;

    public Vector2 GetNextPoint(Vector2 currentUnitPosition, float pointPassRadius) {
        Vector2 closestPoint;
        int index = path.GetClosestPointOnPath(currentUnitPosition, out closestPoint);
        if(index == -1)
            return Vector2.positiveInfinity;
        
        Vector2 result = GetWaypointByIndexClamped(index + 1);
        bool passedThisPoint = (result - currentUnitPosition).CompareLength(pointPassRadius) < 0;
        if(passedThisPoint) {
            result = GetWaypointByIndexClamped(index + 2);
        }
        return result;
    }

    public bool IsLastPointPassed(Vector2 currentUnitPosition, float pointPassRadius) {
        return (currentUnitPosition - end).CompareLength(pointPassRadius) < 0;
    }

    public Vector2 GetWaypointByIndexClamped(int index) {
        return path.GetWaypointByIndexClamped(index);
    }

    public Vector2 GetPointAlongPathClamped(float distance) {
        return path.GetPointAlongPathClamped(distance);
    }

    public Vector2 GetDirectionAtPointAlongPathClamped(float distance) {
        return path.GetDirectionAtPointAlongPathClamped(distance);
    }

    public float GetTotalLength() {
        return path.GetTotalLength();
    }

    public Vector2 GetClosestPointOnPath(Vector2 point) {
        return path.GetClosestPointOnPath(point);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if(!drawGizmos || !path.isValid) {
            return;
        }

        Gizmos.color = Color.yellow;
        for(int i = 0; i < waypoints.Count; i++) {
            Gizmos.DrawSphere(waypoints[i], 1.0f);
            if(i < waypoints.Count - 1) {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);

                Vector2 vector = waypoints[i + 1] - waypoints[i];
                const float arrowsInterval = 13f;
                const float angle = 15;
                const float length = 3;
                Vector2 arrowLine1 = Quaternion.AngleAxis(180 - angle, Vector3.forward) * vector.normalized * length;
                Vector2 arrowLine2 = Quaternion.AngleAxis(180 + angle, Vector3.forward) * vector.normalized * length;
                int arrows = Mathf.Max(1, Mathf.FloorToInt(vector.magnitude / arrowsInterval));
                for(int j = 1; j <= arrows; j++) {
                    Vector2 arrowOrigin = waypoints[i] + vector / (arrows + 1) * j;
                    Gizmos.DrawRay(arrowOrigin, arrowLine1);
                    Gizmos.DrawRay(arrowOrigin, arrowLine2);
                }
            }
        }
    }
#endif
}
