using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatedUnit : MovingUnit {
    protected static readonly int moveXHash = Animator.StringToHash("MoveX");
    protected static readonly int moveYHash = Animator.StringToHash("MoveY");
    protected static readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");
    protected static readonly int angularVelocityHash = Animator.StringToHash("AngularVelocity");
    protected Animator animator;
    private Transform body;

    private int sidewaysDirectionSign = 1;
    private float sidewaysDirectionSignSmooth = 1f;
    private const float sidewaysDirectionInverseDuration = 1f;
    private IEnumerator sidewaysDirectionInverseRoutine = null;

    protected override void Awake() {
        base.Awake();
        animator = GetComponent<Animator>();
        animator.keepAnimatorControllerStateOnDisable = true;
        body = transform.GetChild(0);
    }

    protected override void OnDied() {
        if(actionOnDeath == ActionOnDeath.MakeInactive) {
            animator.keepAnimatorControllerStateOnDisable = false;
        }
        base.OnDied();
    }

    protected override void OnUpdate() {
        base.OnUpdate();
        UpdateMoveAnimation(velocityTrend, angularVelocityTrend);
    }

    protected virtual Vector2 TransformVectorRelativelyBody(Vector2 vector) {
        return body.InverseTransformDirection(vector);
    }

    protected virtual void UpdateMoveAnimation(Vector2 velocity, float angularVelocity) {
        animator.SetFloat(moveSpeedHash, velocity.magnitude);
        velocity = TransformVectorRelativelyBody(velocity);
        velocity.Normalize();

        if((velocity.y < 0 ^ sidewaysDirectionSign < 0) && Mathf.Abs(velocity.y) > 0.1f) { // 0.1f is threshold, used to reduce animation jerks
            if(sidewaysDirectionInverseRoutine != null) {
                StopCoroutine(sidewaysDirectionInverseRoutine);
            }
            sidewaysDirectionInverseRoutine = InvertSidewaysDirection(); // invert sideways anim if we're walking backwards
            StartCoroutine(sidewaysDirectionInverseRoutine);
        }
        velocity.x *= sidewaysDirectionSignSmooth;

        animator.SetFloat(moveXHash, velocity.x);
        animator.SetFloat(moveYHash, velocity.y);

        if(calcAngularVelocityTrend) {
            animator.SetFloat(angularVelocityHash, angularVelocity);
        }
    }

    protected IEnumerator InvertSidewaysDirection() {
        sidewaysDirectionSign = -sidewaysDirectionSign;

        float from = sidewaysDirectionSignSmooth;
        float to = sidewaysDirectionSign;
        for(float timer = 0; timer < sidewaysDirectionInverseDuration; timer += Time.deltaTime) {
            float t = timer / sidewaysDirectionInverseDuration;
            sidewaysDirectionSignSmooth = Mathf.Lerp(from, to, t);
            yield return null;
        }
        sidewaysDirectionSignSmooth = to;
    }
}
