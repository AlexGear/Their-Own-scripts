using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPath {
    List<Vector2> waypoints { get; set; }
    bool isValid { get; }
    float GetTotalLength();
    Vector2 start { get; }
    Vector2 end { get; }
    Vector2 GetWaypointByIndexClamped(int index);
    Vector2 GetPointAlongPathClamped(float distance);
    Vector2 GetDirectionAtPointAlongPathClamped(float distance);
    Vector2 GetClosestPointOnPath(Vector2 point);
}
