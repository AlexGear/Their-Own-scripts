using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class SplineAutoTangents : MonoBehaviour {
#if UNITY_EDITOR
    public ShapeTangentMode replaceWhat = ShapeTangentMode.Broken;
    public ShapeTangentMode replaceWith = ShapeTangentMode.Continuous;

    private SpriteShapeController controller;
    private int pointCount;

    private void Awake() {
        controller = GetComponent<SpriteShapeController>();
    }

    private void Update() {
        if(controller == null) {
            return;
        }
        Spline spline = controller.spline;
        if(pointCount != spline.GetPointCount()) {
            pointCount = spline.GetPointCount();
            for(int i = 0; i < pointCount; i++) {
                if(spline.GetTangentMode(i) == replaceWhat) {
                    spline.SetTangentMode(i, replaceWith);
                }
            }
        }
    }
#endif
}
