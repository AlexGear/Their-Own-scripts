using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension {
    public static int CompareLength(this Vector2 vector, float length) {
        float sqrMagnitude = vector.sqrMagnitude;
        float sqrLength = length * length;
        if(sqrMagnitude < sqrLength) {
            return -1;
        }
        if(sqrMagnitude > sqrLength) {
            return 1;
        }
        return 0;
    }

    public static int CompareLength(this Vector3 vector, float length) {
        float sqrMagnitude = vector.sqrMagnitude;
        float sqrLength = length * length;
        if(sqrMagnitude < sqrLength) {
            return -1;
        }
        if(sqrMagnitude > sqrLength) {
            return 1;
        }
        return 0;
    }

    public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max) {
        return Vector2.Max(min, Vector2.Min(vector, max));
    }

    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max) {
        return Vector3.Max(min, Vector3.Min(vector, max));
    }

    public static bool CloserToPointThan(this Vector2 v, Vector2 point, Vector2 comparedTo) {
        return (point - v).sqrMagnitude < (point - comparedTo).sqrMagnitude;
    }

    public static bool CloserToPointThan(this Vector3 v, Vector3 point, Vector3 comparedTo) {
        return (point - v).sqrMagnitude < (point - comparedTo).sqrMagnitude;
    }

    public static bool IsFinite(this Vector2 v) {
        return !float.IsInfinity(v.x) && !float.IsInfinity(v.y);
    }

    public static bool IsFinite(this Vector3 v) {
        return !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }
}
