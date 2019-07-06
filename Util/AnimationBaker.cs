using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class AnimationBaker : MonoBehaviour {
#if UNITY_EDITOR

    [SerializeField] AnimationClip[] clipsToBake;
    [SerializeField] bool checkToStart;


    private void Update() {
        if(checkToStart) {
            checkToStart = false;
            foreach(var clip in clipsToBake) {
                Bake(clip);
            }
        }
    }
    
    private class TransformCurve {
        public string path;
        public Transform transform;
        public AnimationCurve[] localEulerCurves;

        private float prevEulerZ;

        public TransformCurve(Transform animationRoot, Transform transform) {
            this.transform = transform;
            prevEulerZ = transform.localEulerAngles.z;
            CreateCurves();
            path = AnimationUtility.CalculateTransformPath(transform, animationRoot);
        }

        private void CreateCurves() {
            localEulerCurves = new AnimationCurve[3];
            for(int i = 0; i < 3; i++) {
                localEulerCurves[i] = new AnimationCurve();
            }
        }

        private Vector3 FixAngle360Wrapping(Vector3 euler) {
            float deltaZ = euler.z - prevEulerZ;
            while(Mathf.Abs(deltaZ) > 180) {
                euler.z -= 360 * Mathf.Sign(deltaZ);
                deltaZ = euler.z - prevEulerZ;
            }
            return euler;
        }

        public void RecordFrame(float time) {
            Vector3 localEuler = FixAngle360Wrapping(transform.localEulerAngles);            
            prevEulerZ = localEuler.z;
            for(int i = 0; i < 3; i++) {
                localEulerCurves[i].AddKey(time, localEuler[i]);
            }
        }

        private const string swizzle = "xyzw";

        public void ApplyToClip(AnimationClip clip) {
            for(int i = 0; i < 3; i++) {
                clip.SetCurve(path, typeof(Transform), "localEulerAnglesBaked." + swizzle[i], localEulerCurves[i]);
            }
            clip.EnsureQuaternionContinuity();
        }
    }

    private Transform[] GetIKinfluencedTransforms(IKLimb[] iks) {
        var selves = iks.Select(x => x.transform);
        var lowers = iks.Select(x => x.transform.parent);
        var uppers = lowers.Select(x => x.parent);
        return selves.Concat(lowers.Concat(uppers)).Distinct().ToArray();
    }

    private TransformCurve[] CreateTransformCurves(IKLimb[] iks) {
        Transform[] transforms = GetIKinfluencedTransforms(iks);
        Transform root = this.transform;
        TransformCurve[] tCurves = new TransformCurve[transforms.Length];
        for(int i = 0; i < transforms.Length; i++) {
            tCurves[i] = new TransformCurve(root, transforms[i]);
        }
        return tCurves;
    }

    private void Bake(AnimationClip clipToBake) {
        IKLimb[] iks = this.GetComponentsInChildren<IKLimb>();
        TransformCurve[] transformCurves = CreateTransformCurves(iks);
        int numSteps = Mathf.CeilToInt(clipToBake.frameRate * clipToBake.length);
        float time = 0;
        for(int i = 0; i <= numSteps; i++) {
            clipToBake.SampleAnimation(this.gameObject, time);
            
            foreach(var ik in iks) {
                ik.LateUpdate();
            }
            foreach(var tCurve in transformCurves) {
                tCurve.RecordFrame(time);
            }

            time += 1f / clipToBake.frameRate;
        }

        foreach(var tCurve in transformCurves) {
            tCurve.ApplyToClip(clipToBake);
        }
    }

#endif
}
