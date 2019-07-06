using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingUnit : Unit {
    [SerializeField] protected bool calcAngularVelocityTrend = true;
    protected const float velocityTrendUpdateSpeed = 10f;
    protected const float angularVelocityTrendUpdateSpeed = 20f;

    protected Vector3 lastPosition;
    protected float lastAngle;
    public Vector3 actualVelocity { get; protected set; }
    public Vector3 velocityTrend { get; protected set; }
    public float actualAngularVelocity { get; protected set; }
    public float angularVelocityTrend { get; protected set; }
    public float rotationAngle => transform.eulerAngles.z;

    protected override void Awake() {
        base.Awake();
        lastPosition = transform.position;
        lastAngle = rotationAngle;
    }

    protected override void OnUpdate() {
        base.OnUpdate();
        float deltaTime = Time.deltaTime;
        if(Mathf.Approximately(deltaTime, 0f)) {
            return;
        }

        UpdateVelocityTrend(deltaTime);
        if(calcAngularVelocityTrend) {
            UpdateAngularVelocityTrend(deltaTime);
        }
    }

    protected virtual void UpdateVelocityTrend(float deltaTime) {
        Vector3 position = transform.position;
        actualVelocity = (position - lastPosition) / deltaTime;
        velocityTrend = Vector3.Lerp(velocityTrend, actualVelocity, velocityTrendUpdateSpeed * deltaTime);
        lastPosition = position;
    }

    private void UpdateAngularVelocityTrend(float deltaTime) {
        float angle = rotationAngle;
        actualAngularVelocity = Mathf.DeltaAngle(angle, lastAngle) / deltaTime;
        float t = angularVelocityTrendUpdateSpeed * deltaTime;
        angularVelocityTrend = Mathf.Lerp(angularVelocityTrend, actualAngularVelocity, t);
        lastAngle = angle;
    }
}
