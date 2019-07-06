using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BrushPlacer : MonoBehaviour {
#if UNITY_EDITOR
    private Vector3 pos;

    private void OnDrawGizmosSelected() {
        Handles.BeginGUI();
        var p = Camera.main.WorldToScreenPoint(pos);
        pos = Handles.FreeMoveHandle(p, Quaternion.identity, 10, Vector3.zero, Handles.CircleHandleCap);
        pos = Camera.main.ScreenToWorldPoint(pos);
        Handles.EndGUI();
    }
#endif
}
