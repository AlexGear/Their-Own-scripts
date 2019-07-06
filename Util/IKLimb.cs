using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IKLimb : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] bool invert;
#if UNITY_EDITOR
    [SerializeField] bool checkToUpdate = false;
#endif
    [HideInInspector, SerializeField] Quaternion targetRotationOffset;
    [HideInInspector, SerializeField] bool isTargetRotationOffsetSet = false;

    private Transform upperArm;
    private Transform lowerArm;

    private Quaternion upperRotationOffset;
    private Quaternion lowerRotationOffset;

    private float upperLength;
    private float lowerLength;

	void Start () {
        lowerArm = this.transform.parent;
        if(lowerArm != null) {
            upperArm = lowerArm.parent;
        }

        upperLength = Vector2.Distance(lowerArm.position, upperArm.position);
        lowerLength = Vector2.Distance(this.transform.position, lowerArm.position);

        upperRotationOffset = Quaternion.FromToRotation(upperArm.up, lowerArm.position - upperArm.position);
        lowerRotationOffset = Quaternion.FromToRotation(lowerArm.up, this.transform.position - lowerArm.position);
	}

    private void Reset() {
        Start();
    }

    public void LateUpdate() {
#if UNITY_EDITOR
        if(checkToUpdate) {
            checkToUpdate = false;
            Start();
        }
#endif

        if(target == null || lowerArm == null || upperArm == null) {
            return;
        }

#if UNITY_EDITOR
        if(!isTargetRotationOffsetSet) {
            targetRotationOffset = target.transform.rotation * Quaternion.Inverse(target.transform.localRotation);
            isTargetRotationOffsetSet = true;
        }
#endif

        this.transform.localRotation = targetRotationOffset * target.transform.localRotation;

        Vector3 rootToTarget = target.position - upperArm.position;
        float distanceToTarget = rootToTarget.magnitude;
        if(distanceToTarget > upperLength + lowerLength) {
            upperArm.rotation = Quaternion.LookRotation(Vector3.forward, rootToTarget);
            lowerArm.localRotation = Quaternion.identity;
        }
        else {
            float num = rootToTarget.sqrMagnitude + upperLength * upperLength - lowerLength * lowerLength;
            float den = 2 * distanceToTarget * upperLength;
            float cos = num / den;
            float angle = Mathf.Acos(cos) * Mathf.Rad2Deg;
            if(invert) {
                angle = -angle;
            }
            Vector3 upperDir = Quaternion.AngleAxis(angle, Vector3.forward) * rootToTarget;
            upperArm.rotation = Quaternion.LookRotation(Vector3.forward, upperDir) * upperRotationOffset;

            Vector3 lowerDir = target.transform.position - lowerArm.position;
            lowerArm.rotation = Quaternion.LookRotation(Vector3.forward, lowerDir) * lowerRotationOffset;
        }
    }
}
