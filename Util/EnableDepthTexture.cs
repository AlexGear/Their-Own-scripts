using UnityEngine;
[ExecuteInEditMode]
public class EnableDepthTexture : MonoBehaviour {
    void Start() {
        foreach(var cam in GetComponentsInChildren<Camera>()) {
            cam.depthTextureMode = DepthTextureMode.Depth;
        }
#if UNITY_EDITOR
        if(!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null) {
            UnityEditor.SceneView.lastActiveSceneView.camera.depthTextureMode = DepthTextureMode.Depth;
        }
#endif
    }
}