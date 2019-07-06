using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class SceneViewHelper {
    public static bool IsPointVisible(Vector2 point) {
        Camera camera = SceneView.lastActiveSceneView?.camera;
        if(camera == null)
            return false;

        var viewportPoint = camera.WorldToViewportPoint(point);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && 
               viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    public static bool IsRectVisible(Rect rect) {
        Camera camera = SceneView.lastActiveSceneView?.camera;
        if(camera == null)
            return false;

        var min = camera.WorldToViewportPoint(rect.min);
        if(min.x > 1 || min.y > 1)
            return false;

        var max = camera.WorldToViewportPoint(rect.max);
        if(max.x < 0 || max.y < 0)
            return false;

        return true;
    }
}
#endif