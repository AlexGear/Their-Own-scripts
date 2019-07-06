using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayExtension {
    public static Vector2 GetClosestPoint(this Ray ray, Vector2 point) {
        Vector2 onNormal = ray.direction.normalized;
        Vector2 vector = point - (Vector2)ray.origin;
        Vector2 projection = Vector3.Project(vector, onNormal);
        return (Vector2)ray.origin + projection;
    }
}
