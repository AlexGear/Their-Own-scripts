using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Util/" + nameof(CirclePolygonColliderGenerator))]
[RequireComponent(typeof(PolygonCollider2D))]
public class CirclePolygonColliderGenerator : MonoBehaviour {
    PolygonCollider2D polygonCollider;
    public int vertexCount = 12;
    public float radius = 10.24f;

    [ContextMenu("Generate")]
    public void Generate() {
        polygonCollider = GetComponent<PolygonCollider2D>();
        Vector2[] points = new Vector2[vertexCount];
        for (int i = 0; i < vertexCount; i++) {
            points[i].x = radius * Mathf.Cos(2 * Mathf.PI * i / vertexCount);
            points[i].y = radius * Mathf.Sin(2 * Mathf.PI * i / vertexCount);
        }
        polygonCollider.points = points;
    }
}
