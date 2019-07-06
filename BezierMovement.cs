using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bezier;

public class BezierMovement : MonoBehaviour {
    [SerializeField] BezierPath path;
    [SerializeField] float velocity = 1;
    [SerializeField] Transform target;

    private BezierPath.Slider slider;

    private void Awake() {
        slider = new BezierPath.Slider(path);
        target.position = slider.position;
    }

    public void Update() {
        slider.MoveWithVelocity(velocity);
        target.position = slider.position;
    }
    /*public Transform p0, p1, p2;
    public Transform target;
    public CircleCollider2D circle;
    public float velocity = 0;
    [Range(0, 1)] public float t = 0;

	void Update() {
        if(!p0 || !p1 || !p2 || !target)
            return;        

        t += velocity / BezierVelocity(p0.position, p1.position, p2.position, t).magnitude * Time.deltaTime;
        t = Mathf.Clamp01(t);
        target.position = BezierPosition(p0.position, p1.position, p2.position, t);
        
        Move();
    }

    private Vector3 BezierPosition(float t) {
        return BezierPosition(p0.position, p1.position, p2.position, t);
    }

    private Vector3 BezierVelocity(float t) {
        return BezierVelocity(p0.position, p1.position, p2.position, t);
    }

    private Vector3 BezierPosition(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        return t * ((p1 - p0) * (2 - t) + t * (p2 - p1)) + p0;
    }

    private Vector3 BezierVelocity(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        return 2 * (t * (p2 - 2 * p1 + p0) + p1 - p0);
    }

    [ContextMenu("Move")]
    private void Move() {
        if(circle == null) return;

        const float maxDelta = 0.2f;
        for(int i = 0; i < Mathf.CeilToInt(1 / maxDelta); i++) {
            Vector3 z = BezierPosition(t) - circle.transform.position;
            Vector3 v = BezierVelocity(t);
            float m = z.magnitude;
            float delta = m * (m - circle.radius) / Vector3.Dot(v, z);
            t = t - Mathf.Clamp(delta, -maxDelta, maxDelta);
        }

        t = Mathf.Clamp01(t);
        target.position = Vector3.MoveTowards(target.position, BezierPosition(t), velocity * Time.deltaTime);
    }

    private void OnDrawGizmos() {
        Color color = Color.cyan;
        color.a = 0.3f;
        Gizmos.color = color;
        Gizmos.DrawLine(p0.position, p1.position);
        Gizmos.DrawLine(p1.position, p2.position);

        const float step = 0.03f;
        Vector3 prev = p0.position;
        Gizmos.color = Color.yellow;
        for(float x = step; x < 1; x += step) {
            Vector3 pos = BezierPosition(p0.position, p1.position, p2.position, x);
            Gizmos.DrawLine(prev, pos);
            prev = pos;
        }
    }*/
}
