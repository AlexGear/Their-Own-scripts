using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CurveAsset", menuName = "CurveAsset")]
public class CurveAsset : ScriptableObject {
    [SerializeField] AnimationCurve _curve = AnimationCurve.Linear(0, 0.1f, 200, 5);

    public AnimationCurve curve => _curve;

    public float Evaluate(float time) => _curve.Evaluate(time);
}
